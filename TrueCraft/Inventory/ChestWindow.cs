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
    public class ChestWindow : TrueCraft.Core.Inventory.ChestWindow<IServerSlot>,
        IChestWindow, IServerWindow
    {
        public ChestWindow(IItemRepository itemRepository,
            ISlotFactory<IServerSlot> slotFactory,
            sbyte windowID, ISlots<IServerSlot> mainInventory, ISlots<IServerSlot> hotBar,
            IWorld world,
            GlobalVoxelCoordinates location, GlobalVoxelCoordinates otherHalf) :
            base(itemRepository, slotFactory, windowID, mainInventory, hotBar,
                otherHalf != null)
        {
            World = world;
            Location = location;
            OtherHalf = otherHalf;
            Load();
        }

        private void Load()
        {
            NbtCompound entity = World.GetTileEntity(Location);
            ISlots<IServerSlot> chestInventory = this.ChestInventory;
            if (entity != null)
            {
                foreach (var item in (NbtList)entity["Items"])
                {
                    ItemStack slot = ItemStack.FromNbt((NbtCompound)item);
                    chestInventory[slot.Index].Item = slot;
                    chestInventory[slot.Index].SetClean();
                }
            }
            // Add adjacent items
            if (!object.ReferenceEquals(OtherHalf, null))
            {
                entity = World.GetTileEntity(OtherHalf);
                if (entity != null)
                {
                    foreach (var item in (NbtList)entity["Items"])
                    {
                        ItemStack slot = ItemStack.FromNbt((NbtCompound)item);
                        chestInventory[slot.Index + ChestLength].Item = slot;
                        chestInventory[slot.Index].SetClean();
                    }
                }
            }

        }

        public IWorld World { get; }

        public GlobalVoxelCoordinates Location { get; }

        public GlobalVoxelCoordinates OtherHalf { get; }

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
            throw new NotImplementedException();
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
            bool rv;

            if (right)
            {
                if (shift)
                    rv = HandleShiftRightClick(slotIndex, ref itemStaging);
                else
                    rv = HandleRightClick(slotIndex, ref itemStaging);
            }
            else
            {
                if (shift)
                    rv = HandleShiftLeftClick(slotIndex, ref itemStaging);
                else
                    rv = HandleLeftClick(slotIndex, ref itemStaging);
            }

            // TODO: For each Chest Slot changed, we must inform clients other than
            //       the changing client of the new Slot contents.

            // TODO: Is saving on every mouse click a performance problem?
            //       If we don't save on every mouse click, we'll need to be able to send this
            //       window's contents to any new client opening the chest instead of just
            //       reading it from saved data.
            Save();

            return rv;
        }

        protected bool HandleLeftClick(int slotIndex, ref ItemStack itemStaging)
        {
            if (itemStaging.Empty)
            {
                itemStaging = this[slotIndex];
                this[slotIndex] = ItemStack.EmptyStack;
                return true;
            }
            else
            {
                if (this[slotIndex].Empty)
                {
                    this[slotIndex] = itemStaging;
                    itemStaging = ItemStack.EmptyStack;
                    return true;
                }

                if (itemStaging.CanMerge(this[slotIndex]))
                {
                    int maxStack = ItemRepository.GetItemProvider(itemStaging.ID).MaximumStack;
                    int numToPlace = Math.Min(maxStack - this[slotIndex].Count, itemStaging.Count);
                    if (numToPlace > 0)
                    {
                        ItemStack slot = this[slotIndex];
                        this[slotIndex] = new ItemStack(slot.ID, (sbyte)(slot.Count + numToPlace), slot.Metadata, slot.Nbt);
                        itemStaging = itemStaging.GetReducedStack(numToPlace);
                    }
                    return true;
                }
                else
                {
                    ItemStack tmp = this[slotIndex];
                    this[slotIndex] = itemStaging;
                    itemStaging = tmp;
                    return true;
                }
            }
        }

        protected bool HandleShiftLeftClick(int slotIndex, ref ItemStack itemStaging)
        {
            AreaIndices srcArea = (AreaIndices)GetAreaIndex(slotIndex);

            if (srcArea == AreaIndices.ChestArea)
            {
                ItemStack remaining = Hotbar.StoreItemStack(this[slotIndex], true);
                remaining = MainInventory.StoreItemStack(remaining, true);
                remaining = Hotbar.StoreItemStack(remaining, false);
                remaining = MainInventory.StoreItemStack(remaining, false);
                this[slotIndex] = remaining;

                return true;
            }
            else
            {
                ItemStack remaining = this[slotIndex];
                remaining = ChestInventory.StoreItemStack(remaining, true);
                this[slotIndex] = ChestInventory.StoreItemStack(remaining, false);

                return true;
            }
        }

        protected bool HandleRightClick(int slotIndex, ref ItemStack itemStaging)
        {
            ItemStack stack = this[slotIndex];
            if (!itemStaging.Empty)
            {
                if (stack.CanMerge(itemStaging))
                {
                    int maxStack = ItemRepository.GetItemProvider(itemStaging.ID).MaximumStack;
                    if (stack.Count < maxStack)
                    {
                        this[slotIndex] = new ItemStack(itemStaging.ID, (sbyte)(stack.Count + 1), itemStaging.Metadata, itemStaging.Nbt);
                        itemStaging = itemStaging.GetReducedStack(1);
                        return true;
                    }
                    else
                    {
                        // Right-click on compatible, but maxed-out stack.
                        // This is a No-Op.  
                        // It is assumed that the server will return accepted=true in
                        // this case (similarly to such cases in the Inventory Window).
                        return true;
                    }
                }
                else
                {
                    // Right-click on an incompatible slot => exchange stacks.
                    this[slotIndex] = itemStaging;
                    itemStaging = stack;
                    return true;
                }
            }
            else
            {
                // Right-clicking an empty hand on an empty slot is a No-Op.
                // It is assumed that the server will return accepted=true in
                // this case (similarly to such cases in the Inventory Window).
                if (stack.Empty)
                    return true;

                int cnt = stack.Count;
                int numToPickUp = cnt / 2 + (cnt & 0x0001);

                itemStaging = new ItemStack(stack.ID, (sbyte)numToPickUp, stack.Metadata, stack.Nbt);
                this[slotIndex] = stack.GetReducedStack(numToPickUp);

                return true;
            }
        }

        protected bool HandleShiftRightClick(int slotIndex, ref ItemStack itemStaging)
        {
            return HandleShiftLeftClick(slotIndex, ref itemStaging);
        }

        public void Save()
        {
            NbtList entitySelf = new NbtList("Items", NbtTagType.Compound);
            NbtList entityAdjacent = new NbtList("Items", NbtTagType.Compound);

            ISlots<IServerSlot> chestInventory = this.ChestInventory;
            for (int i = 0, iul = chestInventory.Count; i < iul; i++)
            {
                ItemStack item = chestInventory[i].Item;
                if (!item.Empty)
                {
                    if (i < ChestWindow<IServerSlot>.ChestLength)
                    {
                        item.Index = i;
                        entitySelf.Add(item.ToNbt());
                    }
                    else
                    {
                        item.Index = i - ChestWindow<IServerSlot>.ChestLength;
                        entityAdjacent.Add(item.ToNbt());
                    }
                }
            }

            NbtCompound newEntity = World.GetTileEntity(Location);
            if (newEntity == null)
                newEntity = new NbtCompound(new[] { entitySelf });
            else
                newEntity["Items"] = entitySelf;
            World.SetTileEntity(Location, newEntity);

            if (DoubleChest)
            {
                newEntity = World.GetTileEntity(OtherHalf);
                if (newEntity == null)
                    newEntity = new NbtCompound(new[] { entityAdjacent });
                else
                    newEntity["Items"] = entityAdjacent;
                World.SetTileEntity(OtherHalf, newEntity);
            }
        }
    }
}
