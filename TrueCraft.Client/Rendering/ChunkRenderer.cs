using System;
using System.Collections.Generic;
using TrueCraft.Core.World;
using TrueCraft.Core.Logic;

using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace TrueCraft.Client.Rendering
{
    /// <summary>
    /// A daemon of sorts that creates meshes from chunks.
    /// Passing meshes back is NOT thread-safe.
    /// </summary>
    public class ChunkRenderer : Renderer<ReadOnlyChunk>
    {
        public class ChunkSorter : Comparer<Mesh>
        {
            private GlobalVoxelCoordinates _camera;

            public ChunkSorter(GlobalVoxelCoordinates camera)
            {
                _camera = camera;
            }

            public override int Compare(Mesh _x, Mesh _y)
            {
                ReadOnlyChunk x = ((ChunkMesh)_x).Chunk;
                ReadOnlyChunk y = ((ChunkMesh)_y).Chunk;

                double distX = ((GlobalVoxelCoordinates)x.Coordinates).DistanceTo(_camera);
                double distY = ((GlobalVoxelCoordinates)y.Coordinates).DistanceTo(_camera);

                return (int)(distY - distX);
            }
        }

        public int PendingChunks
        {
            get
            {
                return _items.Count + _priorityItems.Count;
            }
        }

        private ReadOnlyWorld World { get; set; }
        private TrueCraftGame Game { get; set; }
        private IBlockRepository BlockRepository { get; set; }

        public ChunkRenderer(ReadOnlyWorld world, TrueCraftGame game, IBlockRepository blockRepository)
            : base()
        {
            World = world;
            BlockRepository = blockRepository;
            Game = game;
        }

        private static readonly Vector3i[] AdjacentCoordinates =
        {
            Vector3i.Up,
            Vector3i.Down,
            Vector3i.North,
            Vector3i.South,
            Vector3i.East,
            Vector3i.West
        };

        private static readonly VisibleFaces[] AdjacentCoordFaces =
        {
            VisibleFaces.Bottom,
            VisibleFaces.Top,
            VisibleFaces.South,
            VisibleFaces.North,
            VisibleFaces.West,
            VisibleFaces.East
        };

        protected override bool TryRender(ReadOnlyChunk item, out Mesh result)
        {
            var state = new RenderState();
            ProcessChunk(World, item, state);

            result = new ChunkMesh(item, Game, state.Verticies.ToArray(),
                state.OpaqueIndicies.ToArray(), state.TransparentIndicies.ToArray());

            return (result != null);
        }

        private class RenderState
        {
            public readonly List<VertexPositionNormalColorTexture> Verticies 
                = new List<VertexPositionNormalColorTexture>();
            public readonly List<int> OpaqueIndicies = new List<int>();
            public readonly List<int> TransparentIndicies = new List<int>();
            public readonly Dictionary<LocalVoxelCoordinates, VisibleFaces> DrawableCoordinates
                = new Dictionary<LocalVoxelCoordinates, VisibleFaces>();
        }

        private void AddBottomBlock(LocalVoxelCoordinates coords, RenderState state, ReadOnlyChunk chunk)
        {
            VisibleFaces desiredFaces = VisibleFaces.None;
            if (coords.X == 0)
                desiredFaces |= VisibleFaces.West;
            else if (coords.X == Chunk.Width - 1)
                desiredFaces |= VisibleFaces.East;
            if (coords.Z == 0)
                desiredFaces |= VisibleFaces.North;
            else if (coords.Z == Chunk.Depth - 1)
                desiredFaces |= VisibleFaces.South;
            if (coords.Y == 0)
                desiredFaces |= VisibleFaces.Bottom;
            else if (coords.Y == Chunk.Depth - 1)
                desiredFaces |= VisibleFaces.Top;

            VisibleFaces faces;
            state.DrawableCoordinates.TryGetValue(coords, out faces);
            faces |= desiredFaces;
            state.DrawableCoordinates[coords] = desiredFaces;
        }

        private void AddAdjacentBlocks(LocalVoxelCoordinates coords, RenderState state, ReadOnlyChunk chunk)
        {
            // Add adjacent blocks
            for (int i = 0; i < AdjacentCoordinates.Length; i++)
            {
                Vector3i adjacent = AdjacentCoordinates[i];
                int nextX = coords.X + adjacent.X;
                int nextY = coords.Y + adjacent.Y;
                int nextZ = coords.Z + adjacent.Z;
                if (nextX < 0 || nextX >= Chunk.Width || nextY < 0 || nextY >= Chunk.Height
                         || nextZ < 0 || nextZ >= Chunk.Depth)
                    continue;

                LocalVoxelCoordinates next = new LocalVoxelCoordinates(nextX, nextY, nextZ);
                var provider = BlockRepository.GetBlockProvider(chunk.GetBlockId(next));
                if (provider.Opaque)
                {
                    VisibleFaces faces;
                    if (!state.DrawableCoordinates.TryGetValue(next, out faces))
                        faces = VisibleFaces.None;
                    faces |= AdjacentCoordFaces[i];
                    state.DrawableCoordinates[next] = faces;
                }
            }
        }

        private void AddTransparentBlock(LocalVoxelCoordinates coords, RenderState state, ReadOnlyChunk chunk)
        {
            // Add adjacent blocks
            VisibleFaces faces = VisibleFaces.None;
            for (int i = 0; i < AdjacentCoordinates.Length; i++)
            {
                Vector3i adjacent = AdjacentCoordinates[i];
                int nextX = coords.X + adjacent.X;
                int nextY = coords.Y + adjacent.Y;
                int nextZ = coords.Z + adjacent.Z;

                if (nextX < 0 || nextX >= Chunk.Width || nextY < 0 || nextY >= Chunk.Height
                         || nextZ < 0 || nextZ >= Chunk.Depth)
                {
                    faces |= AdjacentCoordFaces[i];
                    continue;
                }
                LocalVoxelCoordinates next = new LocalVoxelCoordinates(nextX, nextY, nextZ);
                if (chunk.GetBlockId(next) == 0)
                    faces |= AdjacentCoordFaces[i];
            }
            if (faces != VisibleFaces.None)
                state.DrawableCoordinates[coords] = faces;
        }

        private void UpdateFacesFromAdjacent(LocalVoxelCoordinates adjacent, ReadOnlyChunk chunk,
            VisibleFaces mod, ref VisibleFaces faces)
        {
            if (chunk == null)
                return;
            var provider = BlockRepository.GetBlockProvider(chunk.GetBlockId(adjacent));
            if (!provider.Opaque)
                faces |= mod;
        }

        private void AddChunkBoundaryBlocks(LocalVoxelCoordinates coords, RenderState state, ReadOnlyChunk chunk)
        {
            VisibleFaces faces;
            if (!state.DrawableCoordinates.TryGetValue(coords, out faces))
                faces = VisibleFaces.None;
            VisibleFaces oldFaces = faces;

            GlobalChunkCoordinates thisChunk = chunk.Chunk.Coordinates;
            if (coords.X == 0)
            {
                GlobalChunkCoordinates westChunk = new GlobalChunkCoordinates(thisChunk.X - 1, thisChunk.Z);
                ReadOnlyChunk nextChunk = World.GetChunk(westChunk);
                LocalVoxelCoordinates adjacent = new LocalVoxelCoordinates(Chunk.Width - 1, coords.Y, coords.Z);
                UpdateFacesFromAdjacent(adjacent, nextChunk, VisibleFaces.West, ref faces);
            }
            else if (coords.X == Chunk.Width - 1)
            {
                GlobalChunkCoordinates eastChunk = new GlobalChunkCoordinates(thisChunk.X + 1, thisChunk.Z);
                var nextChunk = World.GetChunk(eastChunk);
                LocalVoxelCoordinates adjacent = new LocalVoxelCoordinates(0, coords.Y, coords.Z);
                UpdateFacesFromAdjacent(adjacent, nextChunk, VisibleFaces.East, ref faces);
            }

            if (coords.Z == 0)
            {
                GlobalChunkCoordinates northChunk = new GlobalChunkCoordinates(thisChunk.X, thisChunk.Z - 1);
                var nextChunk = World.GetChunk(northChunk);
                LocalVoxelCoordinates adjacent = new LocalVoxelCoordinates(coords.X, coords.Y, Chunk.Depth - 1);
                UpdateFacesFromAdjacent(adjacent, nextChunk, VisibleFaces.North, ref faces);
            }
            else if (coords.Z == Chunk.Depth - 1)
            {
                GlobalChunkCoordinates southChunk = new GlobalChunkCoordinates(thisChunk.X, thisChunk.Z + 1);
                var nextChunk = World.GetChunk(southChunk);
                LocalVoxelCoordinates adjacent = new LocalVoxelCoordinates(coords.X, coords.Y, 0);
                UpdateFacesFromAdjacent(adjacent, nextChunk, VisibleFaces.South, ref faces);
            }

            if (oldFaces != faces)
                state.DrawableCoordinates[coords] = faces;
        }

        private void ProcessChunk(ReadOnlyWorld world, ReadOnlyChunk chunk, RenderState state)
        {
            state.Verticies.Clear();
            state.OpaqueIndicies.Clear();
            state.TransparentIndicies.Clear();
            state.DrawableCoordinates.Clear();

            for (byte x = 0; x < Chunk.Width; x++)
            {
                for (byte z = 0; z < Chunk.Depth; z++)
                {
                    for (byte y = 0; y < Chunk.Height; y++)
                    {
                        LocalVoxelCoordinates coords = new LocalVoxelCoordinates(x, y, z);
                        var id = chunk.GetBlockId(coords);
                        var provider = BlockRepository.GetBlockProvider(id);
                        if (id != 0 && coords.Y == 0)
                            AddBottomBlock(coords, state, chunk);
                        if (!provider.Opaque)
                        {
                            AddAdjacentBlocks(coords, state, chunk);
                            if (id != 0)
                                AddTransparentBlock(coords, state, chunk);
                        }
                        else
                        {
                            if (coords.X == 0 || coords.X == Chunk.Width - 1 ||
                                coords.Z == 0 || coords.Z == Chunk.Depth - 1)
                            {
                                AddChunkBoundaryBlocks(coords, state, chunk);
                            }
                        }
                    }
                }
            }

            foreach(var coords in state.DrawableCoordinates)
            {
                LocalVoxelCoordinates c = coords.Key;
                var descriptor = new BlockDescriptor
                {
                    ID = chunk.GetBlockId(c),
                    Metadata = chunk.GetMetadata(c),
                    BlockLight = chunk.GetBlockLight(c),
                    SkyLight = chunk.GetSkyLight(c),
                    Coordinates = GlobalVoxelCoordinates.GetGlobalVoxelCoordinates(chunk.Chunk.Coordinates, c),
                    Chunk = chunk.Chunk
                };
                var provider = BlockRepository.GetBlockProvider(descriptor.ID);
                if (provider.RenderOpaque)
                {
                    int[] i;
                    var v = BlockRenderer.RenderBlock(provider, descriptor, coords.Value,
                        new Vector3(chunk.X * Chunk.Width + c.X, c.Y, chunk.Z * Chunk.Depth + c.Z),
                        state.Verticies.Count, out i);
                    state.Verticies.AddRange(v);
                    state.OpaqueIndicies.AddRange(i);
                }
                else
                {
                    int[] i;
                    var v = BlockRenderer.RenderBlock(provider, descriptor, coords.Value,
                        new Vector3(chunk.X * Chunk.Width + c.X, c.Y, chunk.Z * Chunk.Depth + c.Z),
                        state.Verticies.Count, out i);
                    state.Verticies.AddRange(v);
                    state.TransparentIndicies.AddRange(i);
                }
            }
        }
    }

    [Flags]
    public enum VisibleFaces
    {
        None = 0,
        North = 1,
        South = 2,
        East = 4,
        West = 8,
        Top = 16,
        Bottom = 32,
        All = North | South | East | West | Top | Bottom
    }
}
