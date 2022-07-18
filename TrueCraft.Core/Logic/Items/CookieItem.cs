using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class CookieItem : FoodItem
    {
        public static readonly short ItemID = 0x165;

        public CookieItem(XmlNode node) : base(node)
        {
        }

        public override float Restores { get { return 0.5f; } }
    }
}