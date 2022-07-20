using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class MapItem : ItemProvider
    {
        public static readonly short ItemID = 0x166;

        public MapItem(XmlNode node) : base(node)
        {
        }

        // TODO: This will need (server-side) behaviour
    }
}