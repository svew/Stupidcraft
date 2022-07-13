using System;
using Microsoft.Xna.Framework.Graphics;
using TrueCraft.Client.Rendering;
using Microsoft.Xna.Framework;
using Matrix = Microsoft.Xna.Framework.Matrix;
using XVector3 = Microsoft.Xna.Framework.Vector3;
using TVector3 = TrueCraft.Core.Vector3;
using TRay = TrueCraft.Core.Ray;
using TrueCraft.Core.Logic;

namespace TrueCraft.Client.Modules
{
    public class HighlightModule : IGraphicalModule
    {
        private readonly IServiceLocator _serviceLocator;
        private readonly TrueCraftGame _game;

        private readonly BasicEffect _highlightEffect;
        private readonly AlphaTestEffect _destructionEffect;
        private Mesh _progressMesh;
        private int Progress { get; set; }

        private static readonly RasterizerState RasterizerState;
        private static readonly VertexPositionColor[] CubeVerticies;
        private static readonly short[] CubeIndicies;
        private static readonly BlendState DestructionBlendState;

        static HighlightModule()
        {
            var color = Color.Black;
            CubeVerticies = new[]
            {
                new VertexPositionColor(new XVector3(0, 0, 1), color),
                new VertexPositionColor(new XVector3(1, 0, 1), color),
                new VertexPositionColor(new XVector3(1, 1, 1), color),
                new VertexPositionColor(new XVector3(0, 1, 1), color),
                new VertexPositionColor(new XVector3(0, 0, 0), color),
                new VertexPositionColor(new XVector3(1, 0, 0), color),
                new VertexPositionColor(new XVector3(1, 1, 0), color),
                new VertexPositionColor(new XVector3(0, 1, 0), color)
            };
            CubeIndicies = new short[]
            {
                0, 1,   1, 2,   2, 3,   3, 0,
                0, 4,   4, 7,   7, 6,   6, 2,
                1, 5,   5, 4,   3, 7,   6, 5
            };
            DestructionBlendState = new BlendState
            {
                ColorSourceBlend = Blend.DestinationColor,
                ColorDestinationBlend = Blend.SourceColor,
                AlphaSourceBlend = Blend.DestinationAlpha,
                AlphaDestinationBlend = Blend.SourceAlpha
            };
            RasterizerState = new RasterizerState
            {
                DepthBias = -3,
                SlopeScaleDepthBias = -3
            };
        }

        public HighlightModule(IServiceLocator serviceLocator, TrueCraftGame game)
        {
            _serviceLocator = serviceLocator;
            _game = game;
            _highlightEffect = new BasicEffect(_game.GraphicsDevice);
            _highlightEffect.VertexColorEnabled = true;
            _destructionEffect = new AlphaTestEffect(_game.GraphicsDevice);
            _destructionEffect.Texture = game.TextureMapper!.GetTexture("terrain.png");
            _destructionEffect.ReferenceAlpha = 1;

            _progressMesh = GenerateProgressMesh();
        }

        private Mesh GenerateProgressMesh()
        {
            int[] indicies;
            var texCoords = new Vector2(Progress, 15);
            var texture = new[]
            {
                texCoords + Vector2.UnitX + Vector2.UnitY,
                texCoords + Vector2.UnitY,
                texCoords,
                texCoords + Vector2.UnitX
            };
            for (int i = 0; i < texture.Length; i++)
                texture[i] *= new Vector2(16f / 256f);
            VertexPositionNormalColorTexture[] vertices = BlockRenderer.CreateUniformCube(XVector3.Zero,
                texture, VisibleFaces.All, 0, out indicies, Color.White);
            return new Mesh(_game, vertices, indicies);
        }

        public void Update(GameTime gameTime)
        {
            var direction = XVector3.Transform(XVector3.UnitZ,
            Matrix.CreateRotationX(MathHelper.ToRadians(_game.Client.Pitch)) *
            Matrix.CreateRotationY(MathHelper.ToRadians(-(_game.Client.Yaw - 180) + 180)));

            var cast = VoxelCast.Cast(_game.Client.Dimension,
                new TRay(_game.Camera.Position, new TVector3(direction.X, direction.Y, direction.Z)),
                _serviceLocator.BlockRepository, TrueCraftGame.Reach, TrueCraftGame.Reach + 2);

            if (cast == null)
                _game.HighlightedBlock = null;
            else
            {
                IBlockProvider provider = _serviceLocator.BlockRepository.GetBlockProvider(_game.Client.Dimension.GetBlockID(cast.Item1));
                if (provider.InteractiveBoundingBox != null)
                {
                    var box = provider.InteractiveBoundingBox.Value;

                    _game.HighlightedBlock = cast.Item1;
                    _game.HighlightedBlockFace = cast.Item2;

                    _destructionEffect.World = _highlightEffect.World = Matrix.Identity
                        * Matrix.CreateScale(new XVector3((float)box.Width, (float)box.Height, (float)box.Depth))
                        * Matrix.CreateTranslation(new XVector3((float)box.Min.X, (float)box.Min.Y, (float)box.Min.Z))
                        * Matrix.CreateTranslation(new XVector3(cast.Item1.X, cast.Item1.Y, cast.Item1.Z));
                }
            }
        }

        public void Draw(GameTime gameTime)
        {
            _game.Camera.ApplyTo(_highlightEffect);
            _game.Camera.ApplyTo(_destructionEffect);

            if (!object.ReferenceEquals(_game.HighlightedBlock, null))
            {
                _game.GraphicsDevice.RasterizerState = RasterizerState;
                foreach (var pass in _highlightEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    _highlightEffect.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                        PrimitiveType.LineList, CubeVerticies, 0,
                        CubeVerticies.Length, CubeIndicies, 0, CubeIndicies.Length / 2);
                }
            }
            if (_game.EndDigging != DateTime.MaxValue)
            {
                var diff = _game.EndDigging - DateTime.UtcNow;
                var total = _game.EndDigging - _game.StartDigging;
                var progress = (int)(diff.TotalMilliseconds / total.TotalMilliseconds * 10);
                progress = -(progress - 5) + 5;
                if (progress > 9)
                    progress = 9;

                if (progress != Progress)
                {
                    Progress = progress;
                    _progressMesh = GenerateProgressMesh();
                }

                _game.GraphicsDevice.BlendState = DestructionBlendState;
                _progressMesh.Draw(_destructionEffect);
                _game.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                _game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            }
        }
    }
}
