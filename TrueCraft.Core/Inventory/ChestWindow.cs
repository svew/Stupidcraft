using System;
using TrueCraft.Core;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Windows;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Inventory
{
    /// <summary>
    /// The backing store for a Chest Window.  This is a common base class for
    /// both Server- and Client-side implementations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ChestWindow<T> : Window<T>, IChestWindow<T> where T : ISlot
    {
        // NOTE: the values in this enum must match the order
        //  in which the slot collections are added in the
        //  ChestWindowContentClient and ChestWindowContentServer
        //  constructors.
        public enum AreaIndices
        {
            ChestArea = 0,
            MainArea = 1,
            HotBarArea = 2
        }

        public const int ChestWidth = 9;
        public const int ChestHeight = 3;
        public const int ChestLength = ChestWidth * ChestHeight;

        public ChestWindow(IItemRepository itemRepository,
            ISlotFactory<T> slotFactory,
            sbyte windowID, ISlots<T> mainInventory, ISlots<T> hotBar,
            bool doubleChest) :
            base(itemRepository, windowID, WindowType.Chest,
                doubleChest ? "Large Chest" : "Chest",
                new ISlots<T>[] { GetSlots(itemRepository, slotFactory, doubleChest), mainInventory, hotBar })
        {
            DoubleChest = doubleChest;
            MainSlotIndex = DoubleChest ? 2 * ChestLength : ChestLength;
            HotbarSlotIndex = MainSlotIndex + ChestInventory.Count;
        }

        private static ISlots<T> GetSlots(IItemRepository itemRepository, ISlotFactory<T> slotFactory, bool doubleChest)
        {
            int len = doubleChest ? 2 * ChestLength : ChestLength;
            return new Slots<T>(itemRepository, slotFactory.GetSlots(itemRepository, len), ChestWidth);
        }

        public ISlots<T> ChestInventory
        {
            get
            {
                return Slots[(int)AreaIndices.ChestArea];
            }
        }

        public bool DoubleChest { get; }

        /// <inheritdoc />
        public int ChestSlotIndex { get => 0; }

        public override bool IsOutputSlot(int slotIndex)
        {
            return false;
        }
    }
}
