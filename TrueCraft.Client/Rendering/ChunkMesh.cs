using System;
using Microsoft.Xna.Framework.Graphics;
using TrueCraft.Core.World;
using Microsoft.Xna.Framework;

namespace TrueCraft.Client.Rendering
{
    /// <summary>
    /// 
    /// </summary>
    public class ChunkMesh : Mesh
    {
        /// <summary>
        /// 
        /// </summary>
        public IChunk Chunk { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="chunk"></param>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        public ChunkMesh(TrueCraftGame game, IChunk chunk, VertexPositionNormalColorTexture[] vertices, int[] indices)
            : base(game, vertices, 1)
        {
            Chunk = chunk;
            SetSubmesh(0, indices);
        }

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
            Chunk = chunk;
            SetSubmesh(0, opaqueIndices);
            SetSubmesh(1, transparentIndices);
        }
    }
}