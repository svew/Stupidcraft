using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class CookedFishItem : FoodItem
    {
        public static readonly short ItemID = 0x15E;

        public CookedFishItem(XmlNode node) : base(node)
        {
        }

        public override float Restores { get { return 2.5f; } }
    }
}