using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class ToolItem : ItemProvider, IToolItem
    {
        private readonly ToolMaterial _material;
        private readonly ToolType _type;
        private readonly short _durability;
        private readonly float _damage;

        private const string ToolNodeName = "tool";
        private const string KindNodeName = "kind";
        private const string MaterialNodeName = "material";
        private const string DurabilityNodeName = "durability";
        private const string DamageNodeName = "damage";

        // Parameterless constructor to support testing
        public ToolItem()
        {

        }

        protected ToolItem(XmlNode node) : base(node)
        {
            XmlNode? toolNode = node[ToolNodeName];
            if (toolNode is null)
                throw new ArgumentException($"Missing <{ToolNodeName}> node.");

            XmlNode? kindNode = toolNode.FirstChild;
            if (kindNode is null || kindNode.LocalName != KindNodeName)
                throw new ArgumentException($"Missing <{KindNodeName}> node.");
            _type = ParseKind(kindNode.InnerText);

            XmlNode? materialNode = kindNode.NextSibling;
            if (materialNode is null || materialNode.LocalName != MaterialNodeName)
                throw new ArgumentException($"Missing <{MaterialNodeName}> node.");
            _material = ParseMaterial(materialNode.InnerText);

            XmlNode? durabilityNode = materialNode.NextSibling;
            if (durabilityNode is null || durabilityNode.LocalName != DurabilityNodeName)
                throw new ArgumentException($"Missing <{DurabilityNodeName}> node.");
            _durability = short.Parse(durabilityNode.InnerText);

            XmlNode? damageNode = durabilityNode.NextSibling;
            if (damageNode is null || damageNode.LocalName != DamageNodeName)
                throw new ArgumentException($"Missing <{DamageNodeName}> node.");
            _damage = float.Parse(damageNode.InnerText);
        }

        protected virtual ToolType ParseKind(string kind)
        {
            switch(kind)
            {
                case "None":
                    return ToolType.None;

                case "Pickaxe":
                    return ToolType.Pickaxe;

                case "Axe":
                    return ToolType.Axe;

                case "Shovel":
                    return ToolType.Shovel;

                case "Hoe":
                    return ToolType.Hoe;

                case "Sword":
                    return ToolType.Sword;
            }

            throw new ArgumentException($"Unkown value ('{kind}') for <{KindNodeName}> node.");
        }

        protected virtual ToolMaterial ParseMaterial(string material)
        {
            switch (material)
            {
                case "None":
                    return ToolMaterial.None;

                case "Wood":
                    return ToolMaterial.Wood;

                case "Stone":
                    return ToolMaterial.Stone;

                case "Iron":
                    return ToolMaterial.Iron;

                case "Gold":
                    return ToolMaterial.Gold;

                case "Diamond":
                    return ToolMaterial.Diamond;
            }

            throw new ArgumentException($"Unkown value ('{material}') for <{MaterialNodeName}> node.");
        }

        /// <inheritdoc />
        public virtual ToolMaterial Material { get => _material; }

        /// <inheritdoc />
        public virtual ToolType ToolType { get => _type; }

        /// <inheritdoc />
        public virtual short Durability { get => _durability; }

        /// <inheritdoc />
        public virtual float Damage { get => _damage; }
    }
}