using System;
using System.Collections.Generic;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Logic.Blocks
{
    public class WoodBlock : BlockProvider, IBurnableItem, ISmeltableItem
    {
        public enum WoodType
        {
            Oak = 0,
            Spruce = 1,
            Birch = 2
        }

        public static readonly byte BlockID = 0x11;
        
        public override byte ID { get { return 0x11; } }
        
        public override double BlastResistance { get { return 10; } }

        public override double Hardness { get { return 2; } }

        public override byte Luminance { get { return 0; } }
        
        public override string GetDisplayName(short metadata)
        {
            return "Wood";
        }

        public override bool Flammable { get { return true; } }

        public TimeSpan BurnTime { get { return TimeSpan.FromSeconds(15); } }

        public ItemStack SmeltingOutput { get => new ItemStack(CoalItem.ItemID, 1, (short)CoalItem.MetaData.Charcoal); }

        public override SoundEffectClass SoundEffect
        {
            get
            {
                return SoundEffectClass.Wood;
            }
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(4, 1);
        }

        public override IEnumerable<short> VisibleMetadata
        {
            get
            {
                yield return (short)WoodType.Oak;
                yield return (short)WoodType.Spruce;
                yield return (short)WoodType.Birch;
            }
        }
    }
}
