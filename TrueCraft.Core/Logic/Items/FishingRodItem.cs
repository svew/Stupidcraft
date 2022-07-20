using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class FishingRodItem : ItemProvider, IDurableItem
    {
        public static readonly short ItemID = 0x15A;

        public FishingRodItem(XmlNode node) : base(node)
        {
        }

        /// <inheritdoc />
        public short Durability { get { return 65; } }

        // TODO: requires server-side behavior
        // TODO: requires client-side rendering support.
    }
}