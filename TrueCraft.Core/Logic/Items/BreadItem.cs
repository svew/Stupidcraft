using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class BreadItem : FoodItem
    {
        public static readonly short ItemID = 0x129;

        public BreadItem(XmlNode node) : base(node)
        {
        }

        public override float Restores { get { return 2.5f; } }
    }
}