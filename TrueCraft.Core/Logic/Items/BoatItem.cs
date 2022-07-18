using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class BoatItem : ItemProvider
    {
        public static readonly short ItemID = 0x14D;

        public BoatItem(XmlNode node) : base(node)
        {
        }

        // TODO: Add Behaviour (server-side)
    }
}