using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class StickItem : ItemProvider, IBurnableItem
    {
        public static readonly short ItemID = 0x118;

        public StickItem(XmlNode node) : base(node)
        {
        }

        public TimeSpan BurnTime { get { return TimeSpan.FromSeconds(5); } }
    }
}