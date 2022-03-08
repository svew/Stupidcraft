using System;

namespace TrueCraft.Core.Logic.Blocks
{
    public class DiamondBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x39;
        
        public override byte ID { get { return 0x39; } }
        
        public override double BlastResistance { get { return 30; } }

        public override double Hardness { get { return 5; } }

        public override byte Luminance { get { return 0; } }
        
        public override string GetDisplayName(short metadata)
        {
            return "Block of Diamond";
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(8, 1);
        }

        public override ToolMaterial EffectiveToolMaterials
        {
            get
            {
                return ToolMaterial.Iron | ToolMaterial.Diamond;
            }
        }

        public override ToolType EffectiveTools
        {
            get
            {
                return ToolType.Pickaxe;
            }
        }
    }
}