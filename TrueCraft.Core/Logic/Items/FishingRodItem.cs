using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class FishingRodItem : ToolItem
    {
        public static readonly short ItemID = 0x15A;

        public FishingRodItem(XmlNode node) : base(node)
        {
        }

        public override short Durability { get { return 65; } }
    }
}