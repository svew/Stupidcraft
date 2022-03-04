using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TrueCraft.Core.Logic;

namespace TrueCraft.Client.Rendering
{
    public static class IconRenderer
    {
        private static Mesh[] _blockMeshes = new Mesh[0x100];

        private static BlockIconCacheEntry[] _blockIconCache = new BlockIconCacheEntry[0x100];

        private static BasicEffect _renderEffect;

        public static void CreateBlocks(TrueCraftGame game, IBlockRepository repository)
        {
            for (int i = 0; i < 0x100; i++)
            {
                var provider = repository.GetBlockProvider((byte)i);
                if (provider == null || provider.GetIconTexture(0) != null)
                    continue;

                int[] indices;
                VertexPositionNormalColorTexture[] vertices = BlockRenderer.RenderIcon(provider, out indices);
                Mesh mesh = new Mesh(game, vertices, indices);
                _blockMeshes[provider.ID] = mesh;
            }

            PrepareEffects(game);
        }

        public static void PrepareEffects(TrueCraftGame game)
        {
            _renderEffect = new BasicEffect(game.GraphicsDevice);
            _renderEffect.Texture = game.TextureMapper?.GetTexture("terrain.png");
            _renderEffect.TextureEnabled = true;
            _renderEffect.VertexColorEnabled = true;
            _renderEffect.LightingEnabled = true;
            _renderEffect.DirectionalLight0.Direction = new Vector3(10, -10, -0.8f);
            _renderEffect.DirectionalLight0.DiffuseColor = Color.White.ToVector3();
            _renderEffect.DirectionalLight0.Enabled = true;
            _renderEffect.Projection = Matrix.CreateOrthographic(18, 18, 0.1f, 1000.0f);   // TODO Hard-coded GUI slot size
            _renderEffect.View = Matrix.CreateLookAt(Vector3.UnitZ, Vector3.Zero, Vector3.Up);
        }

        public static void RenderItemIcon(SpriteBatch spriteBatch, Texture2D texture, IItemProvider provider,
            byte metadata, Rectangle destination, Color color)
        {
            var icon = provider.GetIconTexture(metadata);
            var scale = texture.Width / 16;
            var source = new Rectangle(icon.Item1 * scale, icon.Item2 * scale, scale, scale);
            spriteBatch.Draw(texture, destination, source, color);
        }

        public static void RenderBlockIcon(TrueCraftGame game, SpriteBatch spriteBatch, IBlockProvider provider, byte metadata, Rectangle destination)
        {
            BlockIconCacheEntry iconCacheEntry = _blockIconCache[provider.ID];
            while (iconCacheEntry != null && iconCacheEntry.Metadata != metadata && iconCacheEntry.Next != null)
                iconCacheEntry = iconCacheEntry.Next;
            if (iconCacheEntry?.Metadata != metadata)
                iconCacheEntry = null;

            if (iconCacheEntry == null)
            {
                Mesh mesh = _blockMeshes[provider.ID];
                if (mesh != null)
                {
                    _renderEffect.World = Matrix.Identity
                        * Matrix.CreateScale(0.6f)
                        * Matrix.CreateRotationY(-MathHelper.PiOver4)
                        * Matrix.CreateRotationX(MathHelper.ToRadians(30))
                        * Matrix.CreateScale(new Vector3(18, 18, 1));    // TODO Hard-coded GUI slot size

                    RenderTarget2D newIcon = new RenderTarget2D(game.GraphicsDevice, 18, 18);   // TODO hard-coded GUI slot size

                    game.GraphicsDevice.SetRenderTarget(newIcon);
                    game.GraphicsDevice.Clear(Color.Transparent);
                    mesh.Draw(_renderEffect);
                    game.GraphicsDevice.SetRenderTarget(null);

                    iconCacheEntry = new BlockIconCacheEntry(newIcon, metadata);
                    if (_blockIconCache[provider.ID] == null)
                        _blockIconCache[provider.ID] = iconCacheEntry;
                    else
                        _blockIconCache[provider.ID].Append(iconCacheEntry);
                }
            }

            Texture2D icon = iconCacheEntry.Icon;
            Rectangle source = new Rectangle(0, 0, icon.Width, icon.Height);
            spriteBatch.Draw(icon, destination, source, Color.White);
        }

        private class BlockIconCacheEntry
        {
            private readonly Texture2D _icon;
            private readonly short _metadata;
            private BlockIconCacheEntry _next;

            public BlockIconCacheEntry(Texture2D icon, short metadata)
            {
                _icon = icon;
                _metadata = metadata;
                _next = null;
            }

            public Texture2D Icon { get => _icon; }

            public short Metadata { get => _metadata; }

            public BlockIconCacheEntry Next { get => _next; }

            public void Append(BlockIconCacheEntry icon)
            {
                BlockIconCacheEntry last = this;
                while (last._next != null)
                    last = last._next;

                last._next = icon;
            }
        }
    }
}