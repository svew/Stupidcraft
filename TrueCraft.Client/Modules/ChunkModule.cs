using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TrueCraft.Client.Events;
using TrueCraft.Client.Rendering;
using TrueCraft.Core.Lighting;
using TrueCraft.Core.World;

namespace TrueCraft.Client.Modules
{
    public class ChunkModule : IGraphicalModule
    {
        private readonly TrueCraftGame _game;
        public ChunkRenderer ChunkRenderer { get; }
        public int ChunksRendered { get; set; }

        private readonly HashSet<GlobalChunkCoordinates> _activeMeshes;

        private readonly List<ChunkMesh> _chunkMeshes;
        private readonly ConcurrentBag<ChunkMesh> _incomingChunks;
        private Lighting WorldLighting { get; set; }

        private readonly BasicEffect _opaqueEffect;
        private readonly AlphaTestEffect _transparentEffect;

        public ChunkModule(TrueCraftGame game)
        {
            _game = game;

            ChunkRenderer = new ChunkRenderer(_game, _game.Client.Dimension);
            _game.Client.ChunkLoaded += Game_Client_ChunkLoaded;
            _game.Client.ChunkUnloaded += (sender, e) => UnloadChunk(e.Chunk);
            _game.Client.ChunkModified += Game_Client_ChunkModified;
            _game.Client.BlockChanged += Game_Client_BlockChanged;
            ChunkRenderer.MeshCompleted += MeshCompleted;
            ChunkRenderer.Start();
            WorldLighting = new Lighting(_game.Client.Dimension, _game.BlockRepository);

            _opaqueEffect = new BasicEffect(_game.GraphicsDevice);
            _opaqueEffect.TextureEnabled = true;
            _opaqueEffect.Texture = _game.TextureMapper.GetTexture("terrain.png");
            _opaqueEffect.FogEnabled = true;
            _opaqueEffect.FogStart = 0;
            _opaqueEffect.FogEnd = _game.Camera.Frustum.Far.D * 0.8f;
            _opaqueEffect.VertexColorEnabled = true;
            _opaqueEffect.LightingEnabled = true;

            _transparentEffect = new AlphaTestEffect(_game.GraphicsDevice);
            _transparentEffect.AlphaFunction = CompareFunction.Greater;
            _transparentEffect.ReferenceAlpha = 127;
            _transparentEffect.Texture = _game.TextureMapper.GetTexture("terrain.png");
            _transparentEffect.VertexColorEnabled = true;
            _opaqueEffect.LightingEnabled = true;

            _chunkMeshes = new List<ChunkMesh>();
            _incomingChunks = new ConcurrentBag<ChunkMesh>();
            _activeMeshes = new HashSet<GlobalChunkCoordinates>();
        }

        private void Game_Client_BlockChanged(object? sender, BlockChangeEventArgs e)
        {
            WorldLighting.EnqueueOperation(new TrueCraft.Core.BoundingBox(
                (Core.Vector3)e.Position, (Core.Vector3)(e.Position + Vector3i.One)), false);
            WorldLighting.EnqueueOperation(new TrueCraft.Core.BoundingBox(
                (Core.Vector3)e.Position, (Core.Vector3)(e.Position + Vector3i.One)), true);

            // TODO What is the purpose of enqueuing an entire column after the block?
            //      Could it be related to the lighting height map adding 2 to the height?
            //      That would result in incorrect heights when placing blocks high above other blocks.
            Core.Vector3 posA = new Core.Vector3(e.Position.X, 0, e.Position.Z);
            Core.Vector3 posB = new Core.Vector3(e.Position.X + 1, WorldConstants.Height, e.Position.Z + 1);
            WorldLighting.EnqueueOperation(new TrueCraft.Core.BoundingBox(posA, posB), true);
            WorldLighting.EnqueueOperation(new TrueCraft.Core.BoundingBox(posA, posB), false);
            for (int i = 0; i < 100; i++)
            {
                if (!WorldLighting.TryLightNext())
                    break;
            }

        }

        private void Game_Client_ChunkModified(object? sender, ChunkEventArgs e)
        {
            ChunkRenderer.Enqueue(e.Chunk, true);
        }

        private void Game_Client_ChunkLoaded(object? sender, ChunkEventArgs e)
        {
            ChunkRenderer.Enqueue(e.Chunk);
        }

        private void MeshCompleted(object? sender, RendererEventArgs<IChunk> e)
        {
            _incomingChunks.Add((ChunkMesh)e.Result);
        }

        private void UnloadChunk(IChunk chunk)
        {
            _game.Invoke(() =>
            {
                _activeMeshes.Remove(chunk.Coordinates);
                _chunkMeshes.RemoveAll(m => m.Chunk.Coordinates == chunk.Coordinates);
            });
        }

        private void HandleClientPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Position":
                    ChunkSorter sorter = new ChunkSorter(new GlobalVoxelCoordinates(
                        (int)_game.Client.Position.X, 0, (int)_game.Client.Position.Z));
                    _game.Invoke(() => _chunkMeshes.Sort(sorter));
                    break;
            }
        }

        private class ChunkSorter : Comparer<ChunkMesh>
        {
            private GlobalVoxelCoordinates _camera;

            public ChunkSorter(GlobalVoxelCoordinates camera)
            {
                _camera = camera;
            }

            public override int Compare(ChunkMesh? x, ChunkMesh? y)
            {
                double distX = ((GlobalVoxelCoordinates)x!.Chunk.Coordinates).DistanceTo(_camera);
                double distY = ((GlobalVoxelCoordinates)y!.Chunk.Coordinates).DistanceTo(_camera);

                return (int)(distY - distX);
            }
        }

        public void Update(GameTime gameTime)
        {
            var any = false;
            ChunkMesh? mesh;
            while (_incomingChunks.TryTake(out mesh))
            {
                any = true;
                if (mesh is not null)
                {
                    if (_activeMeshes.Contains(mesh.Chunk.Coordinates))
                    {
                        int existing = _chunkMeshes.FindIndex(m => m.Chunk.Coordinates == mesh.Chunk.Coordinates);
                        _chunkMeshes[existing] = mesh;
                    }
                    else
                    {
                        _activeMeshes.Add(mesh.Chunk.Coordinates);
                        _chunkMeshes.Add(mesh);
                    }
                }
            }
            if (any)
                _game.FlushMainThreadActions();
            WorldLighting.TryLightNext();
        }

        private static readonly BlendState ColorWriteDisable = new BlendState
        {
            ColorWriteChannels = ColorWriteChannels.None
        };

        public void Draw(GameTime gameTime)
        {
            _opaqueEffect.FogColor = _game.SkyModule.WorldFogColor.ToVector3();
            _game.Camera.ApplyTo(_opaqueEffect);
            _game.Camera.ApplyTo(_transparentEffect);
            _opaqueEffect.AmbientLightColor = _transparentEffect.DiffuseColor = Color.White.ToVector3() 
                * new Microsoft.Xna.Framework.Vector3(0.25f + _game.SkyModule.BrightnessModifier);

            int chunks = 0;
            _game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            for (int i = 0; i < _chunkMeshes.Count; i++)
            {
                if (_game.Camera.Frustum.Intersects(_chunkMeshes[i].BoundingBox))
                {
                    chunks++;
                    _chunkMeshes[i].Draw(_opaqueEffect, 0);
                    if (!_chunkMeshes[i].IsReady || _chunkMeshes[i].Submeshes != 2)
                        Console.WriteLine("Warning: rendered chunk that was not ready");
                }
            }

            _game.GraphicsDevice.BlendState = ColorWriteDisable;
            for (int i = 0; i < _chunkMeshes.Count; i++)
            {
                if (_game.Camera.Frustum.Intersects(_chunkMeshes[i].BoundingBox))
                    _chunkMeshes[i].Draw(_transparentEffect, 1);
            }

            _game.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            for (int i = 0; i < _chunkMeshes.Count; i++)
            {
                if (_game.Camera.Frustum.Intersects(_chunkMeshes[i].BoundingBox))
                    _chunkMeshes[i].Draw(_transparentEffect, 1);
            }

            ChunksRendered = chunks;
        }
    }
}
