using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class ClayItem : ItemProvider
    {
        public static readonly short ItemID = 0x151;

        public ClayItem(XmlNode node) : base(node)
        {
        }

        // TODO: Check if this should be smeltable into a brick.
    }
}