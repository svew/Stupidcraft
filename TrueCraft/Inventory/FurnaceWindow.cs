using System;
using System.Collections.Generic;
using fNbt;
using TrueCraft.Core;
using TrueCraft.Core.Inventory;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Inventory
{
    public class FurnaceWindow : Window<IServerSlot>, IFurnaceWindow<IServerSlot>,
        IServerWindow
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
            ISlotFactory<IServerSlot> slotFactory, sbyte windowID, IFurnaceSlots furnaceSlots,
            ISlots<IServerSlot> mainInventory,
            ISlots<IServerSlot> hotBar, IWorld world, GlobalVoxelCoordinates location) :
            base(itemRepository, windowID, Core.Windows.WindowType.Furnace, "Furnace",
                new ISlots<IServerSlot>[] { new ServerSlots(itemRepository, new List<IServerSlot> {furnaceSlots.IngredientSlot }),
                    new ServerSlots(itemRepository, new List<IServerSlot> { furnaceSlots.FuelSlot }),
                    new ServerSlots(itemRepository, new List<IServerSlot> {furnaceSlots.OutputSlot }),
                    mainInventory, hotBar })
        {
            int slotIndex = 0;
            IngredientSlotIndex = slotIndex;
            slotIndex += Ingredient.Count;
            FuelSlotIndex = slotIndex;
            slotIndex += Fuel.Count;
            OutputSlotIndex = slotIndex;
        }

        public ISlots<IServerSlot> Ingredient => Slots[(int)AreaIndices.Ingredient];

        /// <inheritdoc />
        public int IngredientSlotIndex { get; }

        public ISlots<IServerSlot> Fuel => Slots[(int)AreaIndices.Fuel];

        /// <inheritdoc />
        public int FuelSlotIndex { get; }

        public ISlots<IServerSlot> Output => Slots[(int)AreaIndices.Output];

        /// <inheritdoc />
        public int OutputSlotIndex { get; }

        public override bool IsOutputSlot(int slotIndex)
        {
            return slotIndex == _outputSlotIndex;
        }

        /// <inheritdoc />
        public CloseWindowPacket GetCloseWindowPacket()
        {
            return new CloseWindowPacket(WindowID);
        }

        public List<SetSlotPacket> GetDirtySetSlotPackets()
        {
            throw new NotImplementedException();
        }

        public OpenWindowPacket GetOpenWindowPacket()
        {
            int len = Count - MainInventory.Count - Hotbar.Count;
            return new OpenWindowPacket(WindowID, Type, Name, (sbyte)len);
        }

        public WindowItemsPacket GetWindowItemsPacket()
        {
            return new WindowItemsPacket(WindowID, AllItems());
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
                    Slots[j][k].SetClean();
                    index++;
                }
        }

        public bool HandleClick(int slotIndex, bool right, bool shift, ref ItemStack itemStaging)
        {
            if (right)
            {
                if (shift)
                    return HandleShiftRightClick(slotIndex, ref itemStaging);
                else
                    return HandleRightClick(slotIndex, ref itemStaging);
            }
            else
            {
                if (shift)
                    return HandleShiftLeftClick(slotIndex, ref itemStaging);
                else
                    return HandleLeftClick(slotIndex, ref itemStaging);
            }
        }

        protected bool HandleLeftClick(int slotIndex, ref ItemStack itemStaging)
        {
            if (IsOutputSlot(slotIndex))
            {
                // can only remove from output slot.
                ItemStack output = this[slotIndex];

                // It is a No-Op if either the output slot is empty or the output
                // is not compatible with the item in hand.
                // It is assumed that Beta 1.7.3 sends a window click anyway in this case.
                if (output.Empty || !output.CanMerge(itemStaging))
                    return true;

                short itemID = output.ID;
                short metadata = output.Metadata;
                NbtCompound nbt = output.Nbt;
                int maxStack = ItemRepository.GetItemProvider(itemID).MaximumStack;
                int numToPickUp = Math.Min(maxStack - itemStaging.Count, output.Count);

                itemStaging = new ItemStack(itemID, (sbyte)(itemStaging.Count + numToPickUp), metadata, nbt);
                this[slotIndex] = output.GetReducedStack(numToPickUp);
                return true;
            }

            // Play-testing of Beta 1.7.3 shows
            //  - Anything can be placed in the Fuel Slot.
            //  - Anything can be placed in the Ingredient Slot
            //  Smelting begins if the item is burnable AND the Ingredient is smeltable.

            ItemStack slotContent = this[slotIndex];

            if (slotContent.Empty || itemStaging.Empty || !slotContent.CanMerge(itemStaging))
            {
                this[slotIndex] = itemStaging;
                itemStaging = slotContent;
                return true;
            }
            else
            {
                int maxStack = ItemRepository.GetItemProvider(itemStaging.ID).MaximumStack;
                int numToPlace = Math.Min(maxStack - slotContent.Count, itemStaging.Count);
                this[slotIndex] = new ItemStack(slotContent.ID, (sbyte)(slotContent.Count + numToPlace),
                    slotContent.Metadata, slotContent.Nbt);
                itemStaging = itemStaging.GetReducedStack(numToPlace);
                return true;
            }
        }

        protected bool HandleShiftLeftClick(int slotIndex, ref ItemStack itemStaging)
        {
            // TODO
            throw new NotImplementedException();
        }

        protected bool HandleRightClick(int slotIndex, ref ItemStack itemStaging)
        {
            // TODO
            throw new NotImplementedException();
        }

        protected bool HandleShiftRightClick(int slotIndex, ref ItemStack itemStaging)
        {
            return HandleShiftLeftClick(slotIndex, ref itemStaging);
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }
}
