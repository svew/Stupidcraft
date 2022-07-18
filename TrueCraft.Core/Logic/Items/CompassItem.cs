using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class CompassItem : ToolItem
    {
        public static readonly short ItemID = 0x159;

        public CompassItem(XmlNode node) : base(node)
        {
        }
    }
}