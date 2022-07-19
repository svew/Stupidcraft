using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class ArmorItem : ItemProvider, IArmorItem
    {
        private readonly ArmorKind _kind;
        private readonly ArmorMaterial _material;
        private readonly short _durability;
        private readonly float _defencePoints;

        // NOTE: The values of these constants must match the corresponding
        //       node names in TrueCraft.xsd.
        private const string ArmorNodeName = "armor";
        private const string KindNodeName = "kind";
        private const string MaterialNodeName = "material";
        private const string DurabilityNodeName = "durability";
        private const string DefencePointsNodeName = "defencepoints";

        public ArmorItem(XmlNode node) : base(node)
        {
            XmlNode? armorNode = node[ArmorNodeName];
            if (armorNode is null)
                throw new ArgumentException($"Missing {ArmorNodeName} node.");

            XmlNode? kindNode = armorNode[KindNodeName];
            if (kindNode is null)
                throw new ArgumentException($"Missing <{KindNodeName}> node.");
            _kind = ParseArmorKind(kindNode.InnerText);

            XmlNode? materialNode = kindNode.NextSibling;
            if (materialNode is null)
                throw new ArgumentException($"Missing {MaterialNodeName} node.");
            _material = ParseArmorMaterial(materialNode.InnerText);

            XmlNode? durabilityNode = materialNode.NextSibling;
            if (durabilityNode is null)
                throw new ArgumentException($"Missing {DurabilityNodeName} node.");
            _durability = short.Parse(durabilityNode.InnerText);

            XmlNode? defencePointsNode = durabilityNode.NextSibling;
            if (defencePointsNode is null)
                throw new ArgumentException($"Missing {DefencePointsNodeName} node.");
            _defencePoints = float.Parse(defencePointsNode.InnerText);
        }

        protected virtual ArmorKind ParseArmorKind(string armorKind)
        {
            switch (armorKind)
            {
                case "Helmet":
                    return ArmorKind.Helmet;

                case "Chestplate":
                    return ArmorKind.Chestplate;

                case "Leggings":
                    return ArmorKind.Leggings;

                case "Boots":
                    return ArmorKind.Boots;
            }
            throw new ArgumentException($"Unknown Armor Kind: '{armorKind}'");
        }

        protected virtual ArmorMaterial ParseArmorMaterial(string armorMaterial)
        {
            // Note: the cases in this switch must match the values
            //   for the armorMaterialType in TrueCraft.xsd
            switch (armorMaterial)
            {
                case "Leather":
                    return ArmorMaterial.Leather;

                case "Chain":
                    return ArmorMaterial.Chain;

                case "Iron":
                    return ArmorMaterial.Iron;

                case "Gold":
                    return ArmorMaterial.Gold;

                case "Diamond":
                    return ArmorMaterial.Diamond;
            }

            throw new ArgumentException($"Unknown Armor Material: '{armorMaterial}'");
        }

        /// <inheritdoc />
        public ArmorKind Kind { get => _kind; }

        /// <inheritdoc />
        public ArmorMaterial Material { get => _material; }

        /// <inheritdoc />
        public short Durability { get => _durability; }

        /// <inheritdoc />
        public float DefencePoints { get => _defencePoints; }
    }
}
