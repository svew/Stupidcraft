namespace TrueCraft.Core.Logic.Items
{
    public interface IFoodItem : IItemProvider
    {
        /// <summary>
        /// The amount of health this food restores.
        /// </summary>
        float Restores { get; }
    }
}