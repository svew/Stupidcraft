using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class PaintingItem : ItemProvider
    {
        public static readonly short ItemID = 0x141;

        public PaintingItem(XmlNode node) : base(node)
        {
        }

        // TODO: will require (server-side) behaviour
    }
}