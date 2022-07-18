using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class EggItem : ItemProvider
    {
        public static readonly short ItemID = 0x158;

        public EggItem(XmlNode node) : base(node)
        {
        }

        // TODO: (server-side) Eggs have behaviour (be thrown, possible hatch...)
    }
}