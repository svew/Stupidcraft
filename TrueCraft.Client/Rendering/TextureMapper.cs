using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using TrueCraft.Core;

namespace TrueCraft.Client.Rendering
{
    /// <summary>
    /// Provides mappings from keys to textures.
    /// </summary>
    public sealed class TextureMapper : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly IDictionary<string, Texture2D> Defaults =
            new Dictionary<string, Texture2D>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphicsDevice"></param>
        public static void LoadDefaults(GraphicsDevice graphicsDevice)
        {
            Defaults.Clear();

            Defaults.Add("terrain.png", Texture2D.FromFile(graphicsDevice, "Content/terrain.png"));
            Defaults.Add("gui/items.png", Texture2D.FromFile(graphicsDevice, "Content/items.png"));
            Defaults.Add("gui/gui.png", Texture2D.FromFile(graphicsDevice, "Content/gui.png"));
            Defaults.Add("gui/icons.png", Texture2D.FromFile(graphicsDevice, "Content/icons.png"));
            Defaults.Add("gui/crafting.png", Texture2D.FromFile(graphicsDevice, "Content/crafting.png"));
            Defaults.Add("gui/generic_27.png", Texture2D.FromFile(graphicsDevice, "Content/generic_27.png"));
            Defaults.Add("gui/generic_54.png", Texture2D.FromFile(graphicsDevice, "Content/generic_54.png"));
            Defaults.Add("gui/furnace.png", Texture2D.FromFile(graphicsDevice, "Content/furnace.png"));
            Defaults.Add("gui/inventory.png", Texture2D.FromFile(graphicsDevice, "Content/inventory.png"));
            Defaults.Add("terrain/moon.png", Texture2D.FromFile(graphicsDevice, "Content/moon.png"));
            Defaults.Add("terrain/sun.png", Texture2D.FromFile(graphicsDevice, "Content/sun.png"));
        }

        /// <summary>
        /// 
        /// </summary>
        private GraphicsDevice _device;

        /// <summary>
        /// 
        /// </summary>
        private IDictionary<string, Texture2D> _customs;

        /// <summary>
        /// 
        /// </summary>
        private volatile bool _isDisposed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphicsDevice"></param>
        public TextureMapper(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice is null)
                throw new ArgumentException();

            _device = graphicsDevice;
            _customs = new Dictionary<string, Texture2D>();
            _isDisposed = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="texture"></param>
        public void AddTexture(string key, Texture2D texture)
        {
            if (string.IsNullOrEmpty(key) || (texture is null))
                throw new ArgumentException();

            if (_customs.ContainsKey(key))
                _customs[key] = texture;
            else
                _customs.Add(key, texture);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texturePack"></param>
        public void AddTexturePack(TexturePack texturePack)
        {
            if (texturePack is null)
                return;

            // Make sure to 'silence' errors loading custom texture packs;
            // they're unimportant as we can just use default textures.
            try
            {
                using (Stream strm = new FileStream(Path.Combine(Paths.TexturePacks, texturePack.Name), FileMode.Open, FileAccess.Read))
                using (ZipArchive archive = new ZipArchive(strm))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        var key = entry.Name;
                        if (Path.GetExtension(key) == ".png")
                        {
                            using (Stream stream = entry.Open())
                            {
                                // TODO: why copy this stream to a Memory Stream and then use it?
                                //       Why not just use this Stream?
                                try
                                {
                                    using (var ms = new MemoryStream())
                                    {
                                        CopyStream(stream, ms);
                                        ms.Seek(0, SeekOrigin.Begin);
                                        AddTexture(key, Texture2D.FromStream(_device, ms));
                                    }
                                }
                                catch (Exception ex)
                                {  // TODO: what causes Exceptions to be thrown?  Can we use a more specific type of Exception?
                                    Console.WriteLine("Exception ({0}) occurred while loading {1} from texture pack:\n\n{2}", ex.GetType().ToString(), key, ex);
                                }
                            }
                        }
                    }
                }
            }
            catch { return; }
        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[16*1024];
            int read;
            while((read = input.Read (buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Texture2D GetTexture(string key)
        {
            Texture2D? result = null;
            TryGetTexture(key, out result);
            if (result is null)
                // TODO Load a default texture.
                throw new InvalidOperationException($"Failed to find Texture {key}");

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="texture"></param>
        /// <returns></returns>
        public bool TryGetTexture(string key, out Texture2D? texture)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException();

            bool hasTexture = false;
            texture = null;

            // -> Try to load from custom textures
            Texture2D? customTexture = null;
            bool inCustom = _customs.TryGetValue(key, out customTexture);
            texture = (inCustom) ? customTexture : null;
            hasTexture = inCustom;

            // -> Try to load from default textures
            if (!hasTexture)
            {
                Texture2D? defaultTexture = null;
                bool inDefault = TextureMapper.Defaults.TryGetValue(key, out defaultTexture);
                texture = (inDefault) ? defaultTexture : null;
                hasTexture = inDefault;
            }

            // -> Fail gracefully
            return hasTexture;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            foreach (var pair in _customs)
                pair.Value.Dispose();

            _customs.Clear();
            _customs = null!;
            _device = null!;
            _isDisposed = true;
        }
    }
}
