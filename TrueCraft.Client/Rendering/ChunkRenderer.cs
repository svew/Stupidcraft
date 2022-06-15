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
    public class ChunkRenderer : Renderer<IChunk>
    {
        public int PendingChunks
        {
            get
            {
                return _items.Count + _priorityItems.Count;
            }
        }

        private IDimension _dimension;
        private TrueCraftGame _game;

        public ChunkRenderer(TrueCraftGame game, IDimension dimension)
            : base()
        {
            _dimension = dimension;
            _game = game;
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

        protected override bool TryRender(IChunk item, out MeshBase result)
        {
            var state = new RenderState();
            ProcessChunk(item, state);

            result = new ChunkMesh(_game, item, state.Verticies.ToArray(),
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

        private void AddBottomBlock(LocalVoxelCoordinates coords, RenderState state, IChunk chunk)
        {
            VisibleFaces desiredFaces = VisibleFaces.None;
            if (coords.X == 0)
                desiredFaces |= VisibleFaces.West;
            else if (coords.X == WorldConstants.ChunkWidth - 1)
                desiredFaces |= VisibleFaces.East;
            if (coords.Z == 0)
                desiredFaces |= VisibleFaces.North;
            else if (coords.Z == WorldConstants.ChunkDepth - 1)
                desiredFaces |= VisibleFaces.South;
            if (coords.Y == 0)
                desiredFaces |= VisibleFaces.Bottom;
            else if (coords.Y == WorldConstants.ChunkDepth - 1)
                desiredFaces |= VisibleFaces.Top;

            VisibleFaces faces;
            state.DrawableCoordinates.TryGetValue(coords, out faces);
            faces |= desiredFaces;
            state.DrawableCoordinates[coords] = desiredFaces;
        }

        private void AddAdjacentBlocks(LocalVoxelCoordinates coords, RenderState state, IChunk chunk)
        {
            // Add adjacent blocks
            IBlockRepository blockRepository = _dimension.BlockRepository;
            for (int i = 0; i < AdjacentCoordinates.Length; i++)
            {
                Vector3i adjacent = AdjacentCoordinates[i];
                int nextX = coords.X + adjacent.X;
                int nextY = coords.Y + adjacent.Y;
                int nextZ = coords.Z + adjacent.Z;
                if (nextX < 0 || nextX >= WorldConstants.ChunkWidth || nextY < 0 || nextY >= WorldConstants.Height
                         || nextZ < 0 || nextZ >= WorldConstants.ChunkDepth)
                    continue;

                LocalVoxelCoordinates next = new LocalVoxelCoordinates(nextX, nextY, nextZ);
                IBlockProvider provider = blockRepository.GetBlockProvider(chunk.GetBlockID(next));
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

        private void AddTransparentBlock(LocalVoxelCoordinates coords, RenderState state, IChunk chunk)
        {
            // Add adjacent blocks
            VisibleFaces faces = VisibleFaces.None;
            for (int i = 0; i < AdjacentCoordinates.Length; i++)
            {
                Vector3i adjacent = AdjacentCoordinates[i];
                int nextX = coords.X + adjacent.X;
                int nextY = coords.Y + adjacent.Y;
                int nextZ = coords.Z + adjacent.Z;

                if (nextX < 0 || nextX >= WorldConstants.ChunkWidth || nextY < 0 || nextY >= WorldConstants.Height
                         || nextZ < 0 || nextZ >= WorldConstants.ChunkDepth)
                {
                    faces |= AdjacentCoordFaces[i];
                    continue;
                }
                LocalVoxelCoordinates next = new LocalVoxelCoordinates(nextX, nextY, nextZ);
                if (chunk.GetBlockID(next) == 0)   // TODO hard-coded Air Block ID.
                    faces |= AdjacentCoordFaces[i];
            }
            if (faces != VisibleFaces.None)
                state.DrawableCoordinates[coords] = faces;
        }

        private void UpdateFacesFromAdjacent(LocalVoxelCoordinates adjacent, IChunk chunk,
            VisibleFaces mod, ref VisibleFaces faces)
        {
            IBlockProvider provider = _dimension.BlockRepository.GetBlockProvider(chunk.GetBlockID(adjacent));
            if (!provider.Opaque)
                faces |= mod;
        }

        private void AddChunkBoundaryBlocks(LocalVoxelCoordinates coords, RenderState state, IChunk chunk)
        {
            VisibleFaces faces;
            if (!state.DrawableCoordinates.TryGetValue(coords, out faces))
                faces = VisibleFaces.None;
            VisibleFaces oldFaces = faces;

            GlobalChunkCoordinates thisChunk = chunk.Coordinates;
            if (coords.X == 0)
            {
                GlobalChunkCoordinates westChunk = new GlobalChunkCoordinates(thisChunk.X - 1, thisChunk.Z);
                IChunk? nextChunk = _dimension.GetChunk(westChunk);
                if (nextChunk is not null)
                {
                    LocalVoxelCoordinates adjacent = new LocalVoxelCoordinates(WorldConstants.ChunkWidth - 1, coords.Y, coords.Z);
                    UpdateFacesFromAdjacent(adjacent, nextChunk, VisibleFaces.West, ref faces);
                }
            }
            else if (coords.X == WorldConstants.ChunkWidth - 1)
            {
                GlobalChunkCoordinates eastChunk = new GlobalChunkCoordinates(thisChunk.X + 1, thisChunk.Z);
                IChunk? nextChunk = _dimension.GetChunk(eastChunk);
                if (nextChunk is not null)
                {
                    LocalVoxelCoordinates adjacent = new LocalVoxelCoordinates(0, coords.Y, coords.Z);
                    UpdateFacesFromAdjacent(adjacent, nextChunk, VisibleFaces.East, ref faces);
                }
            }

            if (coords.Z == 0)
            {
                GlobalChunkCoordinates northChunk = new GlobalChunkCoordinates(thisChunk.X, thisChunk.Z - 1);
                IChunk? nextChunk = _dimension.GetChunk(northChunk);
                if (nextChunk is not null)
                {
                    LocalVoxelCoordinates adjacent = new LocalVoxelCoordinates(coords.X, coords.Y, WorldConstants.ChunkDepth - 1);
                    UpdateFacesFromAdjacent(adjacent, nextChunk, VisibleFaces.North, ref faces);
                }
            }
            else if (coords.Z == WorldConstants.ChunkDepth - 1)
            {
                GlobalChunkCoordinates southChunk = new GlobalChunkCoordinates(thisChunk.X, thisChunk.Z + 1);
                IChunk? nextChunk = _dimension.GetChunk(southChunk);
                if (nextChunk is not null)
                {
                    LocalVoxelCoordinates adjacent = new LocalVoxelCoordinates(coords.X, coords.Y, 0);
                    UpdateFacesFromAdjacent(adjacent, nextChunk, VisibleFaces.South, ref faces);
                }
            }

            if (oldFaces != faces)
                state.DrawableCoordinates[coords] = faces;
        }

        private void ProcessChunk(IChunk chunk, RenderState state)
        {
            state.Verticies.Clear();
            state.OpaqueIndicies.Clear();
            state.TransparentIndicies.Clear();
            state.DrawableCoordinates.Clear();

            IBlockRepository blockRepository = _dimension.BlockRepository;

            for (byte x = 0; x < WorldConstants.ChunkWidth; x++)
            {
                for (byte z = 0; z < WorldConstants.ChunkDepth; z++)
                {
                    for (byte y = 0; y < WorldConstants.Height; y++)
                    {
                        LocalVoxelCoordinates coords = new LocalVoxelCoordinates(x, y, z);
                        byte id = chunk.GetBlockID(coords);
                        IBlockProvider provider = blockRepository.GetBlockProvider(id);
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
                            if (coords.X == 0 || coords.X == WorldConstants.ChunkWidth - 1 ||
                                coords.Z == 0 || coords.Z == WorldConstants.ChunkDepth - 1)
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
                    ID = chunk.GetBlockID(c),
                    Metadata = chunk.GetMetadata(c),
                    BlockLight = chunk.GetBlockLight(c),
                    SkyLight = chunk.GetSkyLight(c),
                    Coordinates = GlobalVoxelCoordinates.GetGlobalVoxelCoordinates(chunk.Coordinates, c),
                    Chunk = chunk
                };
                IBlockProvider provider = blockRepository.GetBlockProvider(descriptor.ID);
                if (provider.RenderOpaque)
                {
                    int[] i;
                    // TODO: fix adhoc inline coordinate conversion
                    VertexPositionNormalColorTexture[] v = BlockRenderer.RenderBlock(provider, descriptor, coords.Value,
                        new Vector3(chunk.X * WorldConstants.ChunkWidth + c.X, c.Y, chunk.Z * WorldConstants.ChunkDepth + c.Z),
                        state.Verticies.Count, out i);
                    state.Verticies.AddRange(v);
                    state.OpaqueIndicies.AddRange(i);
                }
                else
                {
                    int[] i;
                    // TODO: fix adhoc inline coordinate conversion
                    VertexPositionNormalColorTexture[] v = BlockRenderer.RenderBlock(provider, descriptor, coords.Value,
                        new Vector3(chunk.X * WorldConstants.ChunkWidth + c.X, c.Y, chunk.Z * WorldConstants.ChunkDepth + c.Z),
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
