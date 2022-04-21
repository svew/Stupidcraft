using System;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Networking;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class HoeItem : ToolItem
    {
        public override ToolType ToolType
        {
            get
            {
                return ToolType.Hoe;
            }
        }

        public override void ItemUsedOnBlock(GlobalVoxelCoordinates coordinates, ItemStack item, BlockFace face, IDimension dimension, IRemoteClient user)
        {
            var id = dimension.GetBlockID(coordinates);
            if (id == DirtBlock.BlockID || id == GrassBlock.BlockID)
            {
                dimension.SetBlockID(coordinates, FarmlandBlock.BlockID);
                dimension.BlockRepository.GetBlockProvider(FarmlandBlock.BlockID).BlockPlaced(
                    new BlockDescriptor { Coordinates = coordinates }, face, dimension, user);
            }
        }
    }

    public class WoodenHoeItem : HoeItem
    {
        public static readonly short ItemID = 0x122;

        public override short ID { get { return 0x122; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(0, 8);
        }

        public override ToolMaterial Material { get { return ToolMaterial.Wood; } }

        public override short BaseDurability { get { return 60; } }

        public override string GetDisplayName(short metadata)
        {
            return "Wooden Hoe";
        }
    }

    public class StoneHoeItem : HoeItem
    {
        public static readonly short ItemID = 0x123;

        public override short ID { get { return 0x123; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(1, 8);
        }

        public override ToolMaterial Material { get { return ToolMaterial.Stone; } }

        public override short BaseDurability { get { return 132; } }

        public override string GetDisplayName(short metadata)
        {
            return "Stone Hoe";
        }
    }

    public class IronHoeItem : HoeItem
    {
        public static readonly short ItemID = 0x124;

        public override short ID { get { return 0x124; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(2, 8);
        }

        public override ToolMaterial Material { get { return ToolMaterial.Iron; } }

        public override short BaseDurability { get { return 251; } }

        public override string GetDisplayName(short metadata)
        {
            return "Iron Hoe";
        }
    }

    public class GoldenHoeItem : HoeItem
    {
        public static readonly short ItemID = 0x126;

        public override short ID { get { return 0x126; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(4, 8);
        }

        public override ToolMaterial Material { get { return ToolMaterial.Gold; } }

        public override short BaseDurability { get { return 33; } }

        public override string GetDisplayName(short metadata)
        {
            return "Golden Hoe";
        }
    }

    public class DiamondHoeItem : HoeItem
    {
        public static readonly short ItemID = 0x125;

        public override short ID { get { return 0x125; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(3, 8);
        }

        public override ToolMaterial Material { get { return ToolMaterial.Diamond; } }

        public override short BaseDurability { get { return 1562; } }

        public override string GetDisplayName(short metadata)
        {
            return "Diamond Hoe";
        }
    }
}