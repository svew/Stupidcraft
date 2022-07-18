using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class RedstoneRepeaterItem : ItemProvider
    {
        public static readonly short ItemID = 0x164;

        public RedstoneRepeaterItem(XmlNode node) : base(node)
        {
        }
    }
}