using System;
using System.Collections.Generic;
using System.Linq;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Inventory
{
    public class ArmorSlots<T> : Slots<T> where T : ISlot
    {
        // NOTE: the values of these must match the order in which
        //  the Slots are added to this Slots collection.
        public enum SlotIndices
        {
            Headgear = 0,
            Chestplate = 1,
            Pants = 2,
            Footwear = 3
        }

        // This defines the kinds of armor that can go in each slot.
        // The indices of this array match those of the Slots collection.
        private readonly ArmorKind[] _armorKinds = new ArmorKind[] {
            ArmorKind.Helmet, ArmorKind.Chestplate, ArmorKind.Leggings,
            ArmorKind.Boots };

        public ArmorSlots(IItemRepository itemRepository, ISlotFactory<T> slotFactory) :
            base(itemRepository, GetSlots(itemRepository, slotFactory), 1)
        {
        }

        private static List<T> GetSlots(IItemRepository itemRepository, ISlotFactory<T> slotFactory)
        {
            List<T> rv = new List<T>(4);
            for (int j = 0; j < 4; j ++)
                rv.Add(slotFactory.GetSlot(itemRepository));

            return rv;
        }

        public override ItemStack StoreItemStack(ItemStack item, bool topUpOnly)
        {
            if (item.Empty)
                return ItemStack.EmptyStack;

            IArmorItem? itemProvider = _itemRepository.GetItemProvider(item.ID) as IArmorItem;
            if (itemProvider is null)
                return item;

            for (int j = 0; j < 4; j++)
            {
                if (_armorKinds[j] == itemProvider.Kind)
                {
                    if (this[j].Item.Empty)
                    {
                        this[j].Item = item;
                        return ItemStack.EmptyStack;
                    }
                    else
                    {
                        return item;
                    }
                }
            }

            return item;
        }
    }
}
