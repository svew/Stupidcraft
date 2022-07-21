using System;
using TrueCraft.Core.World;
using Microsoft.Xna.Framework;
using TrueCraft.Client.Modelling;

namespace TrueCraft.Client.Rendering
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ChunkMesh : MeshBase
    {
        private readonly IChunk _chunk;

        /// <summary>
        /// 
        /// </summary>
        public IChunk Chunk { get => _chunk; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="chunk"></param>
        /// <param name="vertices"></param>
        /// <param name="opaqueIndices"></param>
        /// <param name="transparentIndices"></param>
        public ChunkMesh(TrueCraftGame game, IChunk chunk, VertexPositionNormalColorTexture[] vertices, int[] opaqueIndices, int[] transparentIndices)
            : base(game, vertices, 2)
        {
            _chunk = chunk;
            SetSubmesh(0, opaqueIndices);
            SetSubmesh(1, transparentIndices);

            BoundingBox = CalculateBoundingBox(_chunk);
        }

        private static BoundingBox CalculateBoundingBox(IChunk chunk)
        {
            // NOTE adhoc inline coordinate conversion.
            return new BoundingBox(
                new Vector3(chunk.X * WorldConstants.ChunkWidth, 0, chunk.Z * WorldConstants.ChunkDepth),
                new Vector3(chunk.X * WorldConstants.ChunkWidth
                    + WorldConstants.ChunkWidth, WorldConstants.Height,
                    chunk.Z * WorldConstants.ChunkDepth + WorldConstants.ChunkDepth));
        }
    }
}

