using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class ArrowItem : ItemProvider
    {
        public static readonly short ItemID = 0x106;

        public ArrowItem(XmlNode node) : base(node)
        {

        }
    }
}