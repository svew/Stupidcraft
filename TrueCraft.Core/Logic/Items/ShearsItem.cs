using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class ShearsItem : ToolItem
    {
        public static readonly short ItemID = 0x167;

        public ShearsItem(XmlNode node) : base(node)
        {
        }

        public override short Durability { get { return 239; } }
    }
}