using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class MusicDiscItem : ItemProvider
    {
        public static readonly short ItemID = 0x8D1;

        public MusicDiscItem(XmlNode node) : base(node)
        {
        }
    }
}