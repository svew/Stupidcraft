using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class IronIngotItem : ItemProvider
    {
        // TODO: once references to this field are removed, this file can be removed.
        public static readonly short ItemID = 0x109;

        public IronIngotItem(XmlNode node) : base(node)
        {
        }
    }
}