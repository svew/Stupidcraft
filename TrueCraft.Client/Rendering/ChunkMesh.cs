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
            : base(game, 1, true)
        {
            Chunk = chunk;
            Vertices = vertices;
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
            : base(game, 2, true)
        {
            Chunk = chunk;
            Vertices = vertices;
            SetSubmesh(0, opaqueIndices);
            SetSubmesh(1, transparentIndices);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        protected override BoundingBox RecalculateBounds(VertexPositionNormalColorTexture[] vertices)
        {
            // TODO fix adhoc inline coordinate conversion.
            return new BoundingBox(
                new Vector3(Chunk.X * WorldConstants.ChunkWidth, 0, Chunk.Z * WorldConstants.ChunkDepth),
                new Vector3(Chunk.X * WorldConstants.ChunkWidth
                    + WorldConstants.ChunkWidth, WorldConstants.Height,
                    Chunk.Z * WorldConstants.ChunkDepth + WorldConstants.ChunkDepth));
        }
    }
}