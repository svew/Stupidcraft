using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class CookedPorkchopItem : FoodItem
    {
        public static readonly short ItemID = 0x140;

        public CookedPorkchopItem(XmlNode node) : base(node)
        {
        }

        public override float Restores { get { return 4; } }
    }
}