using System;
using System.Collections.Generic;
using TrueCraft.Core;
using TrueCraft.Core.Inventory;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.Server;

namespace TrueCraft.Inventory
{
    public class CraftingBenchWindow : TrueCraft.Core.Inventory.CraftingBenchWindow<IServerSlot>,
        IServerWindow
    {
        public CraftingBenchWindow(IItemRepository itemRepository,
            ICraftingRepository craftingRepository, ISlotFactory<IServerSlot> slotFactory,
            sbyte windowID, ISlots<IServerSlot> mainInventory, ISlots<IServerSlot> hotBar,
            string name, int width, int height) :
            base(itemRepository, craftingRepository, slotFactory, windowID, mainInventory, hotBar, name, width, height)
        {
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
            throw new NotImplementedException();
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
            // TODO
            throw new NotImplementedException();
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
