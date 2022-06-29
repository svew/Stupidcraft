using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TrueCraft.Core.Logic;

namespace TrueCraft.Client.Rendering
{
    public static class IconRenderer
    {
        private static CacheEntry<Mesh>[] _blockMeshes = new CacheEntry<Mesh>[0x100];

        private static CacheEntry<Texture2D>[] _blockIconCache = new CacheEntry<Texture2D>[0x100];

        // This is initialized before use, just not in a way detectable by the compiler.
        private static BasicEffect _renderEffect = null!;

        public static void CreateBlocks(TrueCraftGame game, IBlockRepository repository)
        {
            for (int i = 0; i < 0x100; i++)
            {
                var provider = repository.GetBlockProvider((byte)i);
                if (provider == null || provider.GetIconTexture(0) != null)
                    continue;

                int[] indices;
                foreach (short metadata in provider.VisibleMetadata)
                {
                    VertexPositionNormalColorTexture[] vertices = BlockRenderer.RenderIcon(provider, metadata, out indices);
                    Mesh mesh = new Mesh(game, vertices, indices);
                    CacheEntry<Mesh> meshCacheEntry = new CacheEntry<Mesh>(mesh, metadata);
                    if (_blockMeshes[provider.ID] == null)
                        _blockMeshes[provider.ID] = meshCacheEntry;
                    else
                        _blockMeshes[provider.ID].Append(meshCacheEntry);
                }
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
            Tuple<int, int>? icon = provider.GetIconTexture(metadata);
            if (icon is null)
                icon = new Tuple<int, int>(0, 0);  // TODO: can we do a better default?

            var scale = texture.Width / 16;
            var source = new Rectangle(icon.Item1 * scale, icon.Item2 * scale, scale, scale);
            spriteBatch.Draw(texture, destination, source, color);
        }

        public static void RenderBlockIcon(TrueCraftGame game, SpriteBatch spriteBatch, IBlockProvider provider, byte metadata, Rectangle destination)
        {
            CacheEntry<Texture2D>? iconCacheEntry = _blockIconCache[provider.ID]?.Find(metadata);
            if (iconCacheEntry?.Metadata != metadata)
                iconCacheEntry = null;

            if (iconCacheEntry is null)
            {
                // There must be a Mesh for each Block Provider, so we don't test mesh for null.
                Mesh mesh = _blockMeshes[provider.ID].Find(metadata).Value;
                _renderEffect.World = Matrix.Identity
                    * Matrix.CreateScale(0.6f)
                    * Matrix.CreateRotationY(-MathHelper.PiOver4)
                    * Matrix.CreateRotationX(MathHelper.ToRadians(30))
                    * Matrix.CreateScale(new Vector3(18, 18, 1));    // TODO Hard-coded GUI slot size

                RenderTarget2D newIcon = new RenderTarget2D(game.GraphicsDevice, 3 * 18, 3 * 18);   // TODO hard-coded GUI slot size

                game.GraphicsDevice.SetRenderTarget(newIcon);
                game.GraphicsDevice.Clear(Color.Transparent);
                mesh.Draw(_renderEffect);
                game.GraphicsDevice.SetRenderTarget(null);

                iconCacheEntry = new CacheEntry<Texture2D>(newIcon, metadata);
                if (_blockIconCache[provider.ID] == null)
                    _blockIconCache[provider.ID] = iconCacheEntry;
                else
                    _blockIconCache[provider.ID].Append(iconCacheEntry);
            }

            Texture2D icon = iconCacheEntry.Value;
            Rectangle source = new Rectangle(0, 0, icon.Width, icon.Height);
            spriteBatch.Draw(icon, destination, source, Color.White);
        }

        private class CacheEntry<T>
        {
            private readonly T _icon;
            private readonly short _metadata;
            private CacheEntry<T>? _next;

            public CacheEntry(T icon, short metadata)
            {
                _icon = icon;
                _metadata = metadata;
                _next = null;
            }

            public T Value { get => _icon; }

            public short Metadata { get => _metadata; }

            public CacheEntry<T>? Next { get => _next; }

            public void Append(CacheEntry<T> icon)
            {
                CacheEntry<T> last = this;
                while (last._next != null)
                    last = last._next;

                last._next = icon;
            }

            /// <summary>
            /// Finds the CacheEntry within this list with matching Metadata.
            /// </summary>
            /// <param name="metadata">The metadata to match</param>
            /// <returns>The matching CacheEntry or this if there is no match.</returns>
            /// <remarks>
            /// Call this method on the head of the list.  If no matching entry
            /// is found, the this object will be returned as a default.
            /// </remarks>
            public CacheEntry<T> Find(short metadata)
            {
                CacheEntry<T> rv = this;
                while (rv._metadata != metadata && rv._next != null)
                    rv = rv._next;
                if (rv == null || rv._metadata != metadata)
                    return this;
                else
                    return rv;
            }
        }
    }
}