using System;
using System.Xml;

namespace TrueCraft.Core.Logic
{
    public class CraftingRecipe : ICraftingRecipe, IEquatable<ICraftingRecipe>
    {
        private CraftingPattern _input;

        // TODO this needs to be immutable
        private ItemStack _output;

        public CraftingRecipe(XmlNode recipe)
        {
            XmlNode? pattern = recipe.FirstChild;
            if (pattern is null)
                throw new ArgumentException("The given recipe node has no children.");
            _input = CraftingPattern.GetCraftingPattern(pattern)!;

            XmlNode? output = pattern.NextSibling;
            if (output is null)
                throw new ArgumentException("The given recipe has no output.");
            _output = new ItemStack(output);
        }

        #region object overrides
        public override bool Equals(object? obj)
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

        public bool Equals(ICraftingRecipe? other)
        {
            if (other is null)
                return false;

            return this.Output == other.Output && this.Pattern == other.Pattern;
        }
        #endregion
    }

}
