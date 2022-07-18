using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class AppleItem : FoodItem
    {
        public static readonly short ItemID = 0x104;

        public AppleItem(XmlNode node) : base(node)
        {
        }

        public override float Restores { get { return 2; } }
    }
}