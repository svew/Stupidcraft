using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrueCraft.API.Windows;
using TrueCraft.API.Logic;
using TrueCraft.API;
using TrueCraft.API.World;
using TrueCraft.API.Server;

namespace TrueCraft.Core.Windows
{
    public class FurnaceWindowContent : WindowContent
    {
        public IEventScheduler EventScheduler { get; set; }
        public GlobalVoxelCoordinates Coordinates { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainInventory"></param>
        /// <param name="hotBar"></param>
        /// <param name="scheduler"></param>
        /// <param name="coordinates"></param>
        /// <param name="itemRepository"></param>
        public FurnaceWindowContent(ISlots mainInventory, ISlots hotBar,
            IEventScheduler scheduler, GlobalVoxelCoordinates coordinates,
            IItemRepository itemRepository) :
            base(
                new[]
                {
                    new Slots(1, 1, 1),  // Ingredients
                    new Slots(1, 1, 1),  // Fuel
                    new Slots(1, 1, 1),  // Output
                    mainInventory,
                    hotBar
                },
                itemRepository
                )
        {
            EventScheduler = scheduler;
            Coordinates = coordinates;
        }

        // Indices of the area within the Furnace
        // Note that these are the same values as the slot
        // indices only because the areas contain a single slot.
        // They are conceptually different.
        private const int IngredientAreaIndex = 0;
        private const int FuelAreaIndex = 1;
        private const int OutputAreaIndex = 2;
        private const int MainAreaIndex = 3;
        private const int HotbarAreaIndex = 4;

        // Slot Indices within the overall Furnace
        public const short IngredientIndex = 0;
        public const short FuelIndex = 1;
        public const short OutputIndex = 2;
        public const short MainIndex = 3;
        public const short HotbarIndex = 30;    // TODO: implicitly hard-codes the size of the main inventory

        public override string Name
        {
            get
            {
                return "Furnace";
            }
        }

        public override WindowType Type
        {
            get
            {
                return WindowType.Furnace;
            }
        }

        public override short[] ReadOnlySlots
        {
            get
            {
                return new[] { (short)OutputIndex };
            }
        }

        public ISlots Ingredient 
        {
            get { return SlotAreas[IngredientAreaIndex]; }
        }

        public ISlots Fuel
        {
            get { return SlotAreas[FuelAreaIndex]; }
        }

        public ISlots Output
        {
            get { return SlotAreas[OutputAreaIndex]; }
        }

        public ISlots MainInventory
        {
            get { return SlotAreas[MainAreaIndex]; }
        }

        public ISlots Hotbar
        {
            get { return SlotAreas[HotbarAreaIndex]; }
        }

        public override ItemStack[] GetSlots()
        {
            ItemStack[] rv = new ItemStack[3];
            rv[0] = Ingredient[0];
            rv[1] = Fuel[0];
            rv[2] = Output[0];
            return rv;
        }

        protected override ISlots GetLinkedArea(int index, ItemStack slot)
        {
            if (index < MainIndex)
                return MainInventory;
            return Hotbar;
        }

        /// <inheritdoc />
        public override ItemStack MoveItemStack(int index)
        {
            int sourceAreaIndex = GetAreaIndex(index);
            ItemStack remaining = this[index];

            switch(sourceAreaIndex)
            {
                case IngredientAreaIndex:
                case FuelAreaIndex:
                case OutputAreaIndex:
                    remaining = MoveToInventory(remaining);
                    break;

                case MainAreaIndex:
                case HotbarAreaIndex:
                    remaining = MoveToFurnace(remaining);
                    break;

                default:
                    throw new ApplicationException();
            }

            return remaining;
        }

        private ItemStack MoveToInventory(ItemStack source)
        {
            ItemStack remaining = MainInventory.StoreItemStack(source, true);

            if (!remaining.Empty)
                remaining = Hotbar.StoreItemStack(remaining, false);

            if (!remaining.Empty)
                remaining = MainInventory.StoreItemStack(remaining, false);

            return remaining;
        }

        private ItemStack MoveToFurnace(ItemStack source)
        {
            ItemStack remaining = source;
            IItemProvider provider = ItemRepository.GetItemProvider(source.ID);

            if (provider is IBurnableItem)
            {
                remaining = Fuel.StoreItemStack(remaining, false);
                if (remaining.Empty)
                    return ItemStack.EmptyStack;
            }

            if (provider is ISmeltableItem)
            {
                remaining = Ingredient.StoreItemStack(remaining, false);
                if (remaining.Empty)
                    return ItemStack.EmptyStack;
            }

            return remaining;
        }

        public override ItemStack StoreItemStack(ItemStack slot, bool topUpOnly)
        {
            throw new NotImplementedException();
        }
    }
}