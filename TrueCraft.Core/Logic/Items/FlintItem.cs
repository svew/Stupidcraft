using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class FlintItem : ItemProvider
    {
        // TODO: Once references to this field are removed, this file can be removed.
        public static readonly short ItemID = 0x13E;

        public FlintItem(XmlNode node) : base(node)
        {
        }
    }
}