using System;
using System.Linq;
using Microsoft.Xna.Framework;
using TrueCraft.API.Logic;
using TrueCraft.API.World;
using TrueCraft.Core.World;

namespace TrueCraft.Client.Rendering
{
    public class BlockRenderer
    {
        private static BlockRenderer DefaultRenderer = new BlockRenderer();
        private static BlockRenderer[] Renderers = new BlockRenderer[0x100];

        public static void RegisterRenderer(byte id, BlockRenderer renderer)
        {
            Renderers[id] = renderer;
        }

        public static VertexPositionNormalColorTexture[] RenderBlock(IBlockProvider provider, BlockDescriptor descriptor,
            VisibleFaces faces, Vector3 offset, int indiciesOffset, out int[] indicies)
        {
            var textureMap = provider.GetTextureMap(descriptor.Metadata);
            if (textureMap == null)
                textureMap = new Tuple<int, int>(0, 0); // TODO: handle this better
            return Renderers[descriptor.ID].Render(descriptor, offset, faces, textureMap, indiciesOffset, out indicies);
        }

        public virtual VertexPositionNormalColorTexture[] Render(BlockDescriptor descriptor, Vector3 offset,
            VisibleFaces faces, Tuple<int, int> textureMap, int indiciesOffset, out int[] indicies)
        {
            var texCoords = new Vector2(textureMap.Item1, textureMap.Item2);
            var texture = new[]
            {
                texCoords + Vector2.UnitX + Vector2.UnitY,
                texCoords + Vector2.UnitY,
                texCoords,
                texCoords + Vector2.UnitX
            };

            for (int i = 0; i < texture.Length; i++)
                texture[i] *= new Vector2(16f / 256f);

            int[] lighting = GetLighting(descriptor);

            return CreateUniformCube(offset, texture, faces, indiciesOffset, out indicies, Color.White, lighting);
        }

        public static VertexPositionNormalColorTexture[] RenderIcon(IBlockProvider provider, out int[] indices)
        {
            Tuple<int, int> textureMap = provider.GetTextureMap(0);
            if (textureMap == null)
                textureMap = new Tuple<int, int>(0, 0);

            var texCoords = new Vector2(textureMap.Item1, textureMap.Item2);
            var texture = new[]
            {
                texCoords + Vector2.UnitX + Vector2.UnitY,
                texCoords + Vector2.UnitY,
                texCoords,
                texCoords + Vector2.UnitX
            };

            for (int i = 0; i < texture.Length; i++)
                texture[i] *= new Vector2(16f / 256f);

            Vector3 offset = new Vector3(-0.5f);

            return Renderers[provider.ID].Render(offset, texture, out indices);
        }

        public virtual VertexPositionNormalColorTexture[] Render(Vector3 offset, Vector2[] texture, out int[] indices)
        {
            return CreateUniformCube(offset, texture, VisibleFaces.All, 0, out indices, Color.White);
        }

        public static VertexPositionNormalColorTexture[] CreateUniformCube(Vector3 offset, Vector2[] texture,
            VisibleFaces faces, int indiciesOffset, out int[] indicies, Color color, int[] lighting = null)
        {
            faces = VisibleFaces.All; // Temporary
            if (lighting == null)
                lighting = DefaultLighting;

            int totalFaces = 0;
            uint f = (uint)faces;
            while (f != 0)
            {
                if ((f & 1) == 1)
                    totalFaces++;
                f >>= 1;
            }

            indicies = new int[6 * totalFaces];
            var verticies = new VertexPositionNormalColorTexture[4 * totalFaces];
            int[] _indicies;
            int textureIndex = 0;
            int sidesSoFar = 0;
            for (int _side = 0; _side < 6; _side++)
            {
                if ((faces & VisibleForCubeFace[_side]) == 0)
                {
                    textureIndex += 4;
                    continue;
                }
                var lightColor = LightColor.ToVector3() * CubeBrightness[lighting[_side]];

                var side = (CubeFace)_side;
                var quad = CreateQuad(side, offset, texture, textureIndex % texture.Length, indiciesOffset,
                    out _indicies, new Color(lightColor * color.ToVector3()));
                Array.Copy(quad, 0, verticies, sidesSoFar * 4, 4);
                Array.Copy(_indicies, 0, indicies, sidesSoFar * 6, 6);
                textureIndex += 4;
                sidesSoFar++;
            }
            return verticies;
        }

        protected static VertexPositionNormalColorTexture[] CreateQuad(CubeFace face, Vector3 offset,
            Vector2[] texture, int textureOffset, int indiciesOffset, out int[] indicies, Color color)
        {
            indicies = new[] { 0, 1, 3, 1, 2, 3 };
            for (int i = 0; i < indicies.Length; i++)
                indicies[i] += ((int)face * 4) + indiciesOffset;
            var quad = new VertexPositionNormalColorTexture[4];
            var unit = CubeMesh[(int)face];
            var normal = CubeNormals[(int)face];
            var faceColor = new Color(FaceBrightness[(int)face] * color.ToVector3());
            for (int i = 0; i < 4; i++)
            {
                quad[i] = new VertexPositionNormalColorTexture(offset + unit[i], normal, faceColor, texture[textureOffset + i]);
            }
            return quad;
        }

        #region Lighting

        /// <summary>
        /// The per-vertex light color to apply to blocks.
        /// </summary>
        protected static readonly Color LightColor =
            new Color(245, 245, 225);

        /// <summary>
        /// The default lighting information for rendering a block;
        ///  i.e. when the lighting param to CreateUniformCube == null.
        /// </summary>
        protected static readonly int[] DefaultLighting =
            new int[]
            {
                15, 15, 15,
                15, 15, 15
            };

        /// <summary>
        /// The per-face brightness modifier for lighting.
        /// </summary>
        protected static readonly float[] FaceBrightness =
            new float[]
            {
                0.6f, 0.6f, // North / South
                0.8f, 0.8f, // East / West
                1.0f, 0.5f  // Top / Bottom
            };
        
        /// <summary>
        /// The offset coordinates used to get the position of a block for a face.
        /// </summary>
        protected static readonly Vector3i[] FaceCoords =
            {
                Vector3i.South, Vector3i.North,
                Vector3i.East, Vector3i.West,
                Vector3i.Up, Vector3i.Down
            };

        /// <summary>
        /// Maps a light level [0..15] to a brightness modifier for lighting.
        /// </summary>
        protected static readonly float[] CubeBrightness =
            new float[]
            {
                0.050f, 0.067f, 0.085f, 0.106f, // [ 0..3 ]
                0.129f, 0.156f, 0.186f, 0.221f, // [ 4..7 ]
                0.261f, 0.309f, 0.367f, 0.437f, // [ 8..11]
                0.525f, 0.638f, 0.789f, 1.000f //  [12..15]
            };

        /// <summary>
        /// Gets an array describing the lighting of each Cube Face.
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        protected static int[] GetLighting(BlockDescriptor descriptor)
        {
            int[] lighting = new int[(int)CubeFace.Count];
            LocalVoxelCoordinates coords = (LocalVoxelCoordinates)descriptor.Coordinates;
            int localX, localY, localZ;
            for (int i = 0; i < (int)CubeFace.Count; i++)
            {
                localX = coords.X + FaceCoords[i].X;
                localY = coords.Y + FaceCoords[i].Y;
                localZ = coords.Z + FaceCoords[i].Z;
                lighting[i] = GetLight(descriptor.Chunk, localX, localY, localZ);
            }

            return lighting;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="coords"></param>
        /// <returns></returns>
        private static int GetLight(IChunk chunk, int x, int y, int z)
        {
            // Handle top (and bottom) of the world.
            if (y < 0)
                return 0;
            if (y >= Chunk.Height)
                return 15;

            // TODO: Have to return a proper value for light outside the local chunk.
            //      This will require the Renderer to have access to the World object.
            // Handle coordinates outside the chunk.
            if ((x < 0) || (x >= Chunk.Width) ||
                (z < 0) || (z >= Chunk.Depth))
            {
                return 15;
            }

            LocalVoxelCoordinates coords = new LocalVoxelCoordinates(x, y, z);
            return Math.Min(chunk.GetBlockLight(coords) + chunk.GetSkyLight(coords), 15);
        }

        #endregion

        protected enum CubeFace
        {
            PositiveZ = 0,
            NegativeZ = 1,
            PositiveX = 2,
            NegativeX = 3,
            PositiveY = 4,
            NegativeY = 5,
            Count = 6
        }

        protected static readonly VisibleFaces[] VisibleForCubeFace =
        {
            VisibleFaces.South,
            VisibleFaces.North,
            VisibleFaces.East,
            VisibleFaces.West,
            VisibleFaces.Top,
            VisibleFaces.Bottom
        };

        /// <summary>
        /// Specifies the vertices of a cube for each face.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The first index ranges from 0 to 5 and specifies the
        /// face of the cube (as per the CubeFace enumeration).
        /// The second index ranges from 0 to 3 and specifies each
        /// vertex of the face.
        /// </para>
        /// <para>
        /// All vertices are listed in clockwise order when facing
        /// into the cube.
        /// </para>
        /// </remarks>
        protected static readonly Vector3[][] CubeMesh;

        protected static readonly Vector3[] CubeNormals =
        {
            new Vector3(0, 0, 1),
            new Vector3(0, 0, -1),
            new Vector3(1, 0, 0),
            new Vector3(-1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, -1, 0)
        };

        static BlockRenderer()
        {
            for (int i = 0; i < Renderers.Length; i++)
            {
                Renderers[i] = DefaultRenderer;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes().Where(t =>
                    typeof(BlockRenderer).IsAssignableFrom(t) && !t.IsAbstract && t != typeof(BlockRenderer)))
                {
                    Activator.CreateInstance(type); // This is just to call the static initializers
                }
            }

            CubeMesh = new Vector3[6][];

            CubeMesh[(int)CubeFace.PositiveZ] = new[]
            {
                new Vector3(1, 0, 1),
                new Vector3(0, 0, 1),
                new Vector3(0, 1, 1),
                new Vector3(1, 1, 1)
            };

            CubeMesh[(int)CubeFace.NegativeZ] = new[]
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(1, 1, 0),
                new Vector3(0, 1, 0)
            };

            CubeMesh[(int)CubeFace.PositiveX] = new[]
            {
                new Vector3(1, 0, 0),
                new Vector3(1, 0, 1),
                new Vector3(1, 1, 1),
                new Vector3(1, 1, 0)
            };

            CubeMesh[(int)CubeFace.NegativeX] = new[]
            {
                new Vector3(0, 0, 1),
                new Vector3(0, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 1, 1)
            };

            CubeMesh[(int)CubeFace.PositiveY] = new[]
            {
                new Vector3(1, 1, 1),
                new Vector3(0, 1, 1),
                new Vector3(0, 1, 0),
                new Vector3(1, 1, 0)
            };

            CubeMesh[(int)CubeFace.NegativeY] = new[]
            {
                new Vector3(1, 0, 0),
                new Vector3(0, 0, 0),
                new Vector3(0, 0, 1),
                new Vector3(1, 0, 1)
            };
        }
    }
}
