using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class DiamondItem : ItemProvider
    {
        // TODO: Once references to this field are removed, this file can be removed.
        public static readonly short ItemID = 0x108;

        public DiamondItem(XmlNode node) : base(node)
        {
        }
    }
}