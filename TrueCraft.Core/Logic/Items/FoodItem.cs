using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public class FoodItem : ItemProvider, IFoodItem
    {
        // This must match the node name under <itemrepository> in TrueCraft.xsd
        private const string FoodNodeName = "food";

        // This must match the "restores" node name under <food> in TrueCraft.xsd
        private const string RestoresNodeName = "restores";

        private readonly float _restores;

        public FoodItem(XmlNode node) : base(node)
        {
            XmlNode? foodNode = node[FoodNodeName];
            if (foodNode is null)
                throw new ArgumentException($"Missing <{FoodNodeName}> node.");
            XmlNode? restoreNode = foodNode[RestoresNodeName];
            if (restoreNode is null)
                throw new ArgumentException($"Missing <{RestoresNodeName}> node.");

            _restores = float.Parse(restoreNode.InnerText);
        }

        /// <inheritdoc />
        public float Restores { get => _restores; }

        // TODO: requires ItemUsedOn... overrides (server-side)
    }
}