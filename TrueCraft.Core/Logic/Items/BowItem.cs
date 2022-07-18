using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class BowItem : ItemProvider
    {
        public static readonly short ItemID = 0x105;

        public BowItem(XmlNode node) : base(node)
        {
        }

        // TODO: add behaviour (Server-side)
    }
}