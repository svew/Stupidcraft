using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class SnowballItem : ItemProvider
    {
        public static readonly short ItemID = 0x14C;

        public SnowballItem(XmlNode node) : base(node)
        {
        }

        // TODO: Has behavior (can be thrown)
    }
}