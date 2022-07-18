using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class MinecartItem : ItemProvider
    {
        public static readonly short ItemID = 0x148;

        public MinecartItem(XmlNode node) : base(node)
        {
        }
    }

    public class MinecartWithChestItem : MinecartItem
    {
        public static readonly new short ItemID = 0x156;

        public MinecartWithChestItem(XmlNode node) : base(node)
        {
        }
    }

    public class MinecartWithFurnaceItem : MinecartItem
    {
        public static readonly new short ItemID = 0x157;

        public MinecartWithFurnaceItem(XmlNode node) : base(node)
        {
        }
    }
}