using System;
using System.Collections.Generic;
using fNbt;
using TrueCraft.Client.Handlers;
using TrueCraft.Client.Modules;
using TrueCraft.Core;
using TrueCraft.Core.Inventory;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Windows;
using TrueCraft.Core.World;

namespace TrueCraft.Client.Inventory
{
    public class FurnaceWindow : Window<ISlot>, IFurnaceWindow<ISlot>, IFurnaceProgress, IClickHandler
    {
        // NOTE: these values must match the order in which the slots
        //    collections are added in the constructors.
        public enum AreaIndices
        {
            Ingredient = 0,
            Fuel = 1,
            Output = 2,
            Main = 3,
            Hotbar = 4
        }

        private const int _outputSlotIndex = 2;

        public FurnaceWindow(IItemRepository itemRepository,
            ISlotFactory<ISlot> slotFactory, sbyte windowID,
            ISlots<ISlot> mainInventory, ISlots<ISlot> hotBar,
            IWorld world, GlobalVoxelCoordinates location) :
            base(itemRepository, windowID, WindowType.Furnace, "Furnace",
                new ISlots<ISlot>[] { GetSlots(itemRepository, slotFactory),
                    GetSlots(itemRepository, slotFactory),
                    GetSlots(itemRepository, slotFactory),
                    mainInventory, hotBar })
        {
            IngredientSlotIndex = 0;
            FuelSlotIndex = 1;
            OutputSlotIndex = 2;
            MainSlotIndex = 3;
        }

        private static ISlots<ISlot> GetSlots(IItemRepository itemRepository,
            ISlotFactory<ISlot> slotFactory)
        {
            List<ISlot> lst = new List<ISlot>();
            lst.Add(slotFactory.GetSlot(itemRepository));
            return new Slots<ISlot>(itemRepository, lst, 1);
        }

        /// <inheritdoc />
        public int SmeltingProgress { get; set; }

        /// <inheritdoc />
        public int BurnProgress { get; set; }

        public ISlots<ISlot> Ingredient => Slots[(int)AreaIndices.Ingredient];

        /// <inheritdoc />
        public int IngredientSlotIndex { get; }

        public ISlots<ISlot> Fuel => Slots[(int)AreaIndices.Fuel];

        /// <inheritdoc />
        public int FuelSlotIndex { get; }

        public ISlots<ISlot> Output => Slots[(int)AreaIndices.Output];

        /// <inheritdoc />
        public int OutputSlotIndex { get; }

        public override bool IsOutputSlot(int slotIndex)
        {
            return slotIndex == _outputSlotIndex;
        }

        public override void SetSlots(ItemStack[] slotContents)
        {
#if DEBUG
            if (slotContents.Length != Count)
                throw new ApplicationException($"{nameof(slotContents)}.Length has value of {slotContents.Length}, but {Count} was expected.");
#endif
            int index = 0;
            for (int j = 0, jul = Slots.Length; j < jul; j++)
                for (int k = 0, kul = Slots[j].Count; k < kul; k++)
                {
                    Slots[j][k].Item = slotContents[index];
                    index++;
                }
        }

        /// <inheritdoc />
        public ActionConfirmation HandleClick(int slotIndex, bool rightClick, bool shiftClick, IHeldItem heldItem)
        {
            if (rightClick)
            {
                if (shiftClick)
                    return HandleShiftRightClick(slotIndex, heldItem);
                else
                    return HandleRightClick(slotIndex, heldItem);
            }
            else
            {
                if (shiftClick)
                    return HandleShiftLeftClick(slotIndex, heldItem);
                else
                    return HandleLeftClick(slotIndex, heldItem);
            }
        }

        protected ActionConfirmation HandleLeftClick(int slotIndex, IHeldItem heldItem)
        {
            if (IsOutputSlot(slotIndex))
            {
                // can only remove from output slot.
                ItemStack output = this[slotIndex];

                // It is a No-Op if either the output slot is empty or the output
                // is not compatible with the item in hand.
                // It is assumed that Beta 1.7.3 sends a window click anyway in this case.
                // However, the client can be compatible if we don't bother the
                // server about such things.
                if (output.Empty || !output.CanMerge(heldItem.HeldItem))
                    return null;

                return ActionConfirmation.GetActionConfirmation(() =>
                {
                    short itemID = output.ID;
                    short metadata = output.Metadata;
                    NbtCompound nbt = output.Nbt;
                    int maxStack = ItemRepository.GetItemProvider(itemID).MaximumStack;
                    int numToPickUp = Math.Min(maxStack - heldItem.HeldItem.Count, output.Count);

                    heldItem.HeldItem = new ItemStack(itemID, (sbyte)(heldItem.HeldItem.Count + numToPickUp), metadata, nbt);
                    this[slotIndex] = output.GetReducedStack(numToPickUp);
                });
            }

            // Play-testing of Beta 1.7.3 shows
            //  - Anything can be placed in the Fuel Slot.
            //  - Anything can be placed in the Ingredient Slot
            ItemStack slotContent = this[slotIndex];

            if (slotContent.Empty || heldItem.HeldItem.Empty || !slotContent.CanMerge(heldItem.HeldItem))
            {
                return ActionConfirmation.GetActionConfirmation(() =>
                {
                    this[slotIndex] = heldItem.HeldItem;
                    heldItem.HeldItem = slotContent;
                });
            }
            else
            {
                return ActionConfirmation.GetActionConfirmation(() =>
                {
                    int maxStack = ItemRepository.GetItemProvider(heldItem.HeldItem.ID).MaximumStack;
                    int numToPlace = Math.Min(maxStack - slotContent.Count, heldItem.HeldItem.Count);
                    this[slotIndex] = new ItemStack(slotContent.ID, (sbyte)(slotContent.Count + numToPlace),
                        slotContent.Metadata, slotContent.Nbt);
                    heldItem.HeldItem = heldItem.HeldItem.GetReducedStack(numToPlace);
                });
            }
        }

        protected ActionConfirmation HandleShiftLeftClick(int slotIndex, IHeldItem heldItem)
        {
            // TODO
            throw new NotImplementedException();
        }

        protected ActionConfirmation HandleRightClick(int slotIndex, IHeldItem heldItem)
        {
            // TODO
            throw new NotImplementedException();
        }

        protected ActionConfirmation HandleShiftRightClick(int slotIndex, IHeldItem heldItem)
        {
            return HandleShiftLeftClick(slotIndex, heldItem);
        }

    }
}
