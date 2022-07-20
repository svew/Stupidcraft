using System;
using System.Xml;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Networking;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class HoeItem : ToolItem
    {
        public HoeItem(XmlNode node) : base(node)
        {
        }

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

        public WoodenHoeItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Wood; } }

        public override short Durability { get { return 60; } }
    }

    public class StoneHoeItem : HoeItem
    {
        public static readonly short ItemID = 0x123;

        public StoneHoeItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Stone; } }

        public override short Durability { get { return 132; } }
    }

    public class IronHoeItem : HoeItem
    {
        public static readonly short ItemID = 0x124;

        public IronHoeItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Iron; } }

        public override short Durability { get { return 251; } }
    }

    public class GoldenHoeItem : HoeItem
    {
        public static readonly short ItemID = 0x126;

        public GoldenHoeItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Gold; } }

        public override short Durability { get { return 33; } }
    }

    public class DiamondHoeItem : HoeItem
    {
        public static readonly short ItemID = 0x125;

        public DiamondHoeItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Diamond; } }

        public override short Durability { get { return 1562; } }
    }
}