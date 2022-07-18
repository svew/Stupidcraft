using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class MushroomStewItem : FoodItem
    {
        public static readonly short ItemID = 0x11A;

        public MushroomStewItem(XmlNode node) : base(node)
        {
        }

        public override float Restores { get { return 5; } }
    }
}