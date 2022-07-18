using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class SaddleItem : ItemProvider
    {
        public static readonly short ItemID = 0x149;

        public SaddleItem(XmlNode node) : base(node)
        {
        }

        // TODO: requires (server-side) ItemUsedOnEntity.
    }
}