using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class ArmorItem : ItemProvider
    {
        public ArmorItem(XmlNode node) : base(node)
        {
            // TODO: add ArmorMaterial, BaseDurability, and BaseArmor to TrueCraft.xsd, etc.
        }

        public abstract ArmorMaterial Material { get; }

        public virtual short BaseDurability { get { return 0; } }

        public abstract float BaseArmor { get; }

        public override sbyte MaximumStack { get { return 1; } }
    }
}
