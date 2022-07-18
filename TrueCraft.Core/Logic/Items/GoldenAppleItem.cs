using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class GoldenAppleItem : FoodItem
    {
        public static readonly short ItemID = 0x142;

        public GoldenAppleItem(XmlNode node) : base(node)
        {
        }

        public override float Restores { get { return 10; } }
    }
}