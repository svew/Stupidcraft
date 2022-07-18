using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class ToolItem : ItemProvider
    {
        // Parameterless constructor to support testing
        public ToolItem()
        {

        }

        protected ToolItem(XmlNode node) : base(node)
        {
            // TODO: add ToolItem properties to TrueCraft.xsd item repository.
        }

        public virtual ToolMaterial Material { get { return ToolMaterial.None; } }

        public virtual ToolType ToolType { get { return ToolType.None; } }

        public virtual short BaseDurability { get { return 0; } }

        public virtual int Uses
        {
            get
            {
                return BaseDurability;
            }
        }
    }
}