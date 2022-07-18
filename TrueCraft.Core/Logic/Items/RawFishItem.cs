using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class RawFishItem : FoodItem
    {
        public static readonly short ItemID = 0x15D;

        public RawFishItem(XmlNode node) : base(node)
        {
        }

        public override float Restores { get { return 1; } }
    }
}