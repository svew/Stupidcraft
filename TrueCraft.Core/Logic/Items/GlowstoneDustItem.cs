using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class GlowstoneDustItem : ItemProvider
    {
        // TODO: Once references to this field are removed, this file can be removed.
        public static readonly short ItemID = 0x15C;

        public GlowstoneDustItem(XmlNode node) : base(node)
        {
        }
    }
}