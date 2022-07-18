using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class FoodItem : ItemProvider
    {
        public FoodItem(XmlNode node) : base(node)
        {

        }

        /// <summary>
        /// The amount of health this food restores.
        /// </summary>
        public abstract float Restores { get; }  // TODO: add to ItemRepository tag in TrueCraft.xsd.

        //Most foods aren't stackable
        public override sbyte MaximumStack { get { return 1; } }

        // TODO: requires ItemUsedOn... overrides (server-side)
    }
}