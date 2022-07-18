using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class StringItem : ItemProvider
    {
        public static readonly short ItemID = 0x11F;

        public StringItem(XmlNode node) : base(node)
        {
        }

        // TODO: check if string can be placed in Beta 1.7.3
    }
}