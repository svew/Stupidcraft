using System;
using TrueCraft.API.Windows;
using TrueCraft.Core.Windows;
using TrueCraft.API;

namespace TrueCraft.Core.Windows
{
    // TODO Refactoring: Encapsulates InventoryWindow, only uses some of it;
    //          Copies information back and forth.  Confusing for maintenance.
    public class ChestWindow : Window
    {
        public ChestWindow(InventoryWindow inventory, bool doubleChest)
        {
            DoubleChest = doubleChest;
            if (doubleChest)
            {
                WindowAreas = new[]
                {
                    new WindowArea(ChestIndex, 2 * ChestLength, ChestWidth, 2 * ChestHeight), // Chest
                    new WindowArea(DoubleMainIndex, InventoryWindow.InventoryLength,
                             InventoryWindow.InventoryWidth, InventoryWindow.InventoryHeight), // Main inventory
                    new WindowArea(DoubleHotbarIndex, 9, 9, 1) // Hotbar TODO hard-coded constants
                };
            }
            else
            {
                WindowAreas = new[]
                {
                    new WindowArea(ChestIndex, ChestLength, ChestWidth, ChestHeight), // Chest
                    new WindowArea(MainIndex, InventoryWindow.InventoryLength,
                             InventoryWindow.InventoryWidth, InventoryWindow.InventoryHeight), // Main inventory
                    new WindowArea(HotbarIndex, 9, 9, 1) // Hotbar TODO hard-coded constants
                };
            }
            inventory.MainInventory.CopyTo(MainInventory);
            inventory.Hotbar.CopyTo(Hotbar);
            Copying = false;
            inventory.WindowChange += (sender, e) =>
            {
                if (Copying) return;
                if (InventoryWindow.IsPlayerInventorySlot(e.SlotIndex)
                    || InventoryWindow.IsHotbarIndex(e.SlotIndex))
                {
                    inventory.MainInventory.CopyTo(MainInventory);
                    inventory.Hotbar.CopyTo(Hotbar);
                }
            };
            foreach (var area in WindowAreas)
                area.WindowChange += (s, e) => OnWindowChange(new WindowChangeEventArgs(
                    (s as WindowArea).StartIndex + e.SlotIndex, e.Value));
        }

        public const int ChestIndex = 0;
        public const int ChestWidth = 9;
        public const int ChestHeight = 3;
        public const int ChestLength = ChestWidth * ChestHeight;

        public const int DoubleChestSecondaryIndex = 27;
        public const int MainIndex = 27;
        public const int HotbarIndex = 54;
        public const int DoubleMainIndex = 54;
        public const int DoubleHotbarIndex = 81;

        public bool DoubleChest { get; }

        public override IWindowArea[] WindowAreas { get; }

        private bool Copying { get; set; }

        public override string Name
        {
            get
            {
                if (DoubleChest)
                    return "Large Chest";
                return "Chest";
            }
        }

        public override WindowType Type
        {
            get
            {
                return WindowType.Chest;
            }
        }

        public IWindowArea ChestInventory
        {
            get
            {
                return WindowAreas[0];
            }
        }

        public IWindowArea MainInventory
        {
            get
            {
                return WindowAreas[1];
            }
        }

        public IWindowArea Hotbar
        {
            get
            {
                return WindowAreas[2];
            }
        }

        public override int Length2
        {
            get
            {
                return ChestInventory.Length;
            }
        }

        public override void CopyToInventory(IWindow inventoryWindow)
        {
            var window = (InventoryWindow)inventoryWindow;
            Copying = true;
            MainInventory.CopyTo(window.MainInventory);
            Hotbar.CopyTo(window.Hotbar);
            Copying = false;
        }

        protected override IWindowArea GetLinkedArea(int index, ItemStack slot)
        {
            if (index == 0)
                return Hotbar;
            else
                return ChestInventory;
        }
    }
}