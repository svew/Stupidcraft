using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class GoldIngotItem : ItemProvider
    {
        // TODO: Once all references to this field are removed, this file can be removed.
        public static readonly short ItemID = 0x10A;

        public GoldIngotItem(XmlNode node) : base(node)
        {
        }
    }
}