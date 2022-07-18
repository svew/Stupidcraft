using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class WheatItem : ItemProvider
    {
        // TODO: Once references to this field are removed, this file can be removed.
        public static readonly short ItemID = 0x128;

        public WheatItem(XmlNode node) : base(node)
        {
        }
    }
}