namespace TrueCraft.Core.Logic.Items
{
    public interface IToolItem : IDurableItem
    {
        /// <summary>
        /// Gets the ToolMaterial of the ToolItem.
        /// </summary>
        ToolMaterial Material { get; }

        /// <summary>
        /// Gets the type of the ToolItem.
        /// </summary>
        ToolType ToolType { get; }

        /// <summary>
        /// Gets the amount of additional damage dealt to mobs when they are
        /// attacked while the ToolItem is being held by the Player.
        /// </summary>
        float Damage { get; }
    }
}