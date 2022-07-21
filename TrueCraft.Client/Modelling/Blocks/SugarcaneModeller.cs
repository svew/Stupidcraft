using System;
using Microsoft.Xna.Framework;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Client.Modelling.Blocks
{
    public class SugarcaneModeller : FlatQuadModeller
    {
        static SugarcaneModeller()
        {
            RegisterRenderer(SugarcaneBlock.BlockID, new SugarcaneModeller());
        }

        protected override Vector2 TextureMap { get { return new Vector2(9, 4); } }
    }
}