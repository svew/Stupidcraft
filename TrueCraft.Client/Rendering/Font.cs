using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TrueCraft.Client.Rendering
{
    /// <summary>
    /// Represents a font.
    /// </summary>
    public class Font
    {
        private FontFile _definition;
        private string _directory;
        private Texture2D[] _textures;
        private Dictionary<char, FontChar> _glyphs;

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public FontStyle Style { get; private set; }

        // _definitions, _textures & _glyphs are all initialized
        // in private methods called from the constructor.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="directory"></param>
        /// <param name="name"></param>
        /// <param name="style"></param>
        public Font(GraphicsDevice graphicsDevice, string directory, string name, FontStyle style = FontStyle.Regular)
        {
            _directory = directory;
            Name = name;
            Style = style;

            LoadContent(graphicsDevice);
            GenerateGlyphs();
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public Texture2D GetTexture(int page = 0)
        {
            return _textures[page];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public FontChar? GetGlyph(char ch)
        {
            FontChar? glyph = null;
            _glyphs.TryGetValue(ch, out glyph);
            return glyph;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphicsDevice"></param>
        private void LoadContent(GraphicsDevice graphicsDevice)
        {
            var definitionPath = string.Format("{0}_{1}.fnt", Name, Style);
            using (var contents = File.OpenRead(Path.Combine(_directory, definitionPath)))
                _definition = FontLoader.Load(contents);

            // We need to support multiple texture pages for more than plain ASCII text.
            _textures = new Texture2D[_definition.Pages.Count];
            for (int i = 0; i < _definition.Pages.Count; i++)
            {
                var texturePath = Path.Combine(_directory, string.Format("{0}_{1}_{2}.png", Name, Style, i));
                _textures[i] = Texture2D.FromFile(graphicsDevice, texturePath);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void GenerateGlyphs()
        {
            _glyphs = new Dictionary<char, FontChar>(_definition.Chars.Count);
            foreach (var glyph in _definition.Chars)
            {
                char c = (char)glyph.ID;
                _glyphs.Add(c, glyph);
            }
        }
    }
}
