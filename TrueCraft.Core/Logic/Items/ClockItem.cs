using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class ClockItem : ToolItem
    {
        public static readonly short ItemID = 0x15B;

        public ClockItem(XmlNode node) : base(node)
        {
        }
    }
}