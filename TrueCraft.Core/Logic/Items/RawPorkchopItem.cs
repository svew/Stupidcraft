using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class RawPorkchopItem : FoodItem
    {
        public static readonly short ItemID = 0x13F;

        public RawPorkchopItem(XmlNode node) : base(node)
        {
        }

        public override float Restores { get { return 1.5f; } }
    }
}