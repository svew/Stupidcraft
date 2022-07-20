using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class ShearsItem : ItemProvider, IDurableItem
    {
        public static readonly short ItemID = 0x167;

        public ShearsItem(XmlNode node) : base(node)
        {
        }

        /// <inheritdoc />
        public short Durability { get { return 239; } }
    }
}