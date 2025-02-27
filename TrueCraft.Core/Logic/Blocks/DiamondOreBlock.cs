using System;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Logic.Blocks
{
    public class DiamondOreBlock : BlockProvider, ISmeltableItem
    {
        public static readonly byte BlockID = 0x38;
        
        public override byte ID { get { return 0x38; } }
        
        public override double BlastResistance { get { return 15; } }

        public override double Hardness { get { return 3; } }

        public override byte Luminance { get { return 0; } }
        
        public override string GetDisplayName(short metadata)
        {
            return "Diamond Ore";
        }

        public ItemStack SmeltingOutput { get { return new ItemStack((short)ItemIDs.Diamond); } }

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

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(2, 3);
        }

        protected override ItemStack[] GetDrop(BlockDescriptor descriptor, ItemStack item)
        {
            return new[] { new ItemStack((short)ItemIDs.Diamond, 1, descriptor.Metadata) };
        }
    }
}