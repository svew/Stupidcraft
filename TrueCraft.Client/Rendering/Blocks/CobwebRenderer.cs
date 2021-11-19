using System;
using Microsoft.Xna.Framework;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Client.Rendering.Blocks
{
    public class CobwebRenderer : FlatQuadRenderer
    {
        static CobwebRenderer()
        {
            BlockRenderer.RegisterRenderer(CobwebBlock.BlockID, new CobwebRenderer());
        }

        protected override Vector2 TextureMap { get { return new Vector2(11, 0); } }
    }
}
