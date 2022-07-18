using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class CoalItem : ItemProvider, IBurnableItem
    {
        public enum MetaData : short
        {
            Coal = 0,
            Charcoal = 1
        }

        public static readonly short ItemID = 0x107;

        public CoalItem(XmlNode node) : base(node)
        {
        }

        public TimeSpan BurnTime { get { return TimeSpan.FromSeconds(80); } }
    }
}