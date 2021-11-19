using System;

namespace TrueCraft.Core.Windows
{
    public class ArmorSlots : Slots
    {
        public const int Footwear = 3;
        public const int Pants = 2;
        public const int Chestplate = 1;
        public const int Headgear = 0;

        public ArmorSlots() : base(4, 1, 4)
        {
        }

        protected override bool IsValid(ItemStack slot, int index)
        {
            if (slot.Empty)
                return true;
            // TODO: Armor
            //var item = (IArmorItem)(Item)slot.Id;
            //if (item == null)
            //    return false;
            //if (index == Footwear && item.ArmorSlot != ArmorSlot.Footwear)
            //    return false;
            //if (index == Pants && item.ArmorSlot != ArmorSlot.Pants)
            //    return false;
            //if (index == Chestplate && item.ArmorSlot != ArmorSlot.Chestplate)
            //    return false;
            //if (index == Headgear && item.ArmorSlot != ArmorSlot.Headgear)
            //    return false;
            return base.IsValid(slot, index);
        }

        //public override int MoveOrMergeItem(int index, ItemStack slot, ISlots from)
        //{
        //    for (int i = 0; i < Count; i++)
        //    {
        //        if (IsValid(slot, i))
        //        {
        //            if (this[i].Empty)
        //            {
        //                this[i] = slot;
        //                from[index] = ItemStack.EmptyStack;
        //                return i;
        //            }
        //        }
        //    }
        //    return -1;
        //}

        public override ItemStack StoreItemStack(ItemStack item, bool topUpOnly)
        {
            if (item.Empty)
                return ItemStack.EmptyStack;

            ItemStack remaining = item;
            for (int j = 0; j < Count; j ++)
                if (IsValid(item, j) && this[j].Empty)
                {
                    this[j] = item;
                    return ItemStack.EmptyStack;
                }

            return item;
        }
    }
}
