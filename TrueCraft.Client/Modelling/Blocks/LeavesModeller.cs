﻿using System;
using Microsoft.Xna.Framework;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Client.Modelling.Blocks
{
    public class LeavesModeller : BlockModeller
    {
        static LeavesModeller()
        {
            RegisterRenderer(LeavesBlock.BlockID, new LeavesModeller());
            for (int i = 0; i < BaseTextures.Length; i++)
            {
                BaseTextures[i] *= new Vector2(16f / 256f);
                SpruceTextures[i] *= new Vector2(16f / 256f);
            }
        }

        private static Vector2 BaseTexture = new Vector2(4, 3);
        private static Vector2 SpruceTexture = new Vector2(4, 8);
        private static Vector2[] BaseTextures =
        {
            BaseTexture + Vector2.UnitX + Vector2.UnitY,
            BaseTexture + Vector2.UnitY,
            BaseTexture,
            BaseTexture + Vector2.UnitX
        };
        private static Vector2[] SpruceTextures =
        {
            SpruceTexture + Vector2.UnitX + Vector2.UnitY,
            SpruceTexture + Vector2.UnitY,
            SpruceTexture,
            SpruceTexture + Vector2.UnitX
        };

        public override VertexPositionNormalColorTexture[] Render(BlockDescriptor descriptor, Vector3 offset,
            VisibleFaces faces, Tuple<int, int> textureMap, int indiciesOffset, out int[] indicies)
        {
            int[] lighting = GetLighting(descriptor);

            switch ((WoodBlock.WoodType)descriptor.Metadata)
            {
                case WoodBlock.WoodType.Spruce:
                    return CreateUniformCube(offset, SpruceTextures, VisibleFaces.All,
                        indiciesOffset, out indicies, GrassModeller.BiomeColor, lighting);
                case WoodBlock.WoodType.Birch:
                    return CreateUniformCube(offset, BaseTextures, VisibleFaces.All,
                        indiciesOffset, out indicies, GrassModeller.BiomeColor, lighting);
                case WoodBlock.WoodType.Oak:
                default:
                    return CreateUniformCube(offset, BaseTextures, VisibleFaces.All,
                        indiciesOffset, out indicies, GrassModeller.BiomeColor, lighting);
            }
        }
    }
}