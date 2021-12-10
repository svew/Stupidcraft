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

        private readonly short[][] _armorIDs = new short[][] {
            new short[] { LeatherCapItem.ItemID, IronHelmetItem.ItemID, GoldenHelmetItem.ItemID, DiamondHelmetItem.ItemID, ChainHelmetItem.ItemID },
            new short[] { LeatherTunicItem.ItemID, IronChestplateItem.ItemID, GoldenChestplateItem.ItemID, DiamondChestplateItem.ItemID, ChainChestplateItem.ItemID },
            new short[] { LeatherPantsItem.ItemID, IronLeggingsItem.ItemID, GoldenLeggingsItem.ItemID, DiamondLeggingsItem.ItemID, ChainLeggingsItem.ItemID },
            new short[] { LeatherBootsItem.ItemID, IronBootsItem.ItemID, GoldenBootsItem.ItemID, DiamondBootsItem.ItemID, ChainBootsItem.ItemID }
    };

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

            for (int j = 0; j < 4; j++)
                if (_armorIDs[j].Contains(item.ID))
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

            return item;
        }
    }
}
