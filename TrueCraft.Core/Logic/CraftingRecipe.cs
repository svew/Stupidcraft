using System;
using System.Xml;
using TrueCraft.API;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic
{
    public class CraftingRecipe : ICraftingRecipe, IEquatable<ICraftingRecipe>
    {
        private CraftingPattern _input;

        // TODO this needs to be immutable
        private ItemStack _output;

        public CraftingRecipe(XmlNode recipe)
        {
            XmlNode pattern = recipe.FirstChild;
            CraftingPattern _input = CraftingPattern.GetCraftingPattern(pattern);

            XmlNode input = pattern.NextSibling;
            _output = new ItemStack(input);
        }

        #region object overrides
        public override bool Equals(object obj)
        {
            return Equals(obj as ICraftingRecipe);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        #region interface ICraftingRecipe
        public CraftingPattern Pattern { get => _input; }

        public ItemStack Output { get => _output; }

        public bool Equals(ICraftingRecipe other)
        {
            if (object.ReferenceEquals(other, null))
                return false;

            return this.Output == other.Output && this.Pattern == other.Pattern;
        }
        #endregion
    }

}
