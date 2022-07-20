namespace TrueCraft.Core.Logic.Items
{
    public interface IDurableItem : IItemProvider
    {
        /// <summary>
        /// Gets the Durability of the Durable Item when newly crafted.
        /// </summary>
        short Durability { get; }
    }
}