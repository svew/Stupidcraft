﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TrueCraft.Client.Handlers;
using TrueCraft.Client.Input;
using TrueCraft.Client.Inventory;
using TrueCraft.Client.Rendering;
using TrueCraft.Core;
using TrueCraft.Core.Inventory;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.Windows;

namespace TrueCraft.Client.Modules
{
    public class WindowModule : InputModule, IGraphicalModule
    {
        private TrueCraftGame Game { get; }
        private SpriteBatch SpriteBatch { get; }
        private Texture2D Inventory { get; }

        private Texture2D Crafting { get; }

        private Texture2D _chest;
        private Texture2D _doubleChest;

        private Texture2D Items { get; }
        private FontRenderer Font { get; }
        private short SelectedSlot { get; set; }
        private ItemStack HeldItem { get; set; }

        private enum RenderStage
        {
            Sprites,
            Models,
            Text
        }

        public WindowModule(TrueCraftGame game, FontRenderer font)
        {
            Game = game;
            Font = font;
            SpriteBatch = new SpriteBatch(game.GraphicsDevice);

            Inventory = game.TextureMapper.GetTexture("gui/inventory.png");
            Crafting = game.TextureMapper.GetTexture("gui/crafting.png");
            Items = game.TextureMapper.GetTexture("gui/items.png");
            _chest = game.TextureMapper.GetTexture("gui/generic_27.png");
            _doubleChest = game.TextureMapper.GetTexture("gui/generic_54.png");

            SelectedSlot = -1;
            HeldItem = ItemStack.EmptyStack;
        }

        // TODO fix hard-coded constants.
        private static readonly Rectangle InventoryWindowRect = new Rectangle(0, 0, 176, 166);
        private static readonly Rectangle CraftingWindowRect = new Rectangle(0, 0, 176, 166);

        /// <summary>
        /// The size of the single-chest window
        /// </summary>
        /// <remarks>
        /// The dimensions of this Rectangle are dictated by the dimensions of the
        /// Texture.  In turn, this size is chosen for compatibility with MineCraft.
        /// </remarks>
        private static readonly Rectangle _chestWindowRect = new Rectangle(0, 0, 176, 166);

        /// <summary>
        /// The size of the double-chest window
        /// </summary>
        /// <remarks>
        /// The dimensions of this Rectangle are dictated by the dimensions of the
        /// Texture.  In turn, this size is chosen for compatibility with MineCraft.
        /// </remarks>
        private static readonly Rectangle _doubleChestWindowRect = new Rectangle(0, 0, 176, 299);

        public void Draw(GameTime gameTime)
        {
            if (object.ReferenceEquals(Game.Client.CurrentWindow, null))
                return;

            // TODO: slot == -999 when outside of the window and -1 when inside the window, but not on an item
            SelectedSlot = -999;

            var scale = new Point((int)(16 * Game.ScaleFactor * 2));
            var mouse = Mouse.GetState().Position - new Point((int)(8 * Game.ScaleFactor * 2));
            var rect = new Rectangle(mouse, scale);

            IItemProvider provider = null;
            if (!HeldItem.Empty)
                provider = Game.ItemRepository.GetItemProvider(HeldItem.ID);

            SpriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.NonPremultiplied);
            SpriteBatch.Draw(Game.White1x1, new Rectangle(0, 0,
                Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), new Color(Color.Black, 180));

            switch (Game.Client.CurrentWindow.Type)
            {
                case WindowType.Inventory:
                    SpriteBatch.Draw(Inventory, new Vector2(
                        Game.GraphicsDevice.Viewport.Width / 2 - Scale(InventoryWindowRect.Width / 2),
                        Game.GraphicsDevice.Viewport.Height / 2 - Scale(InventoryWindowRect.Height / 2)),
                        InventoryWindowRect, Color.White, 0, Vector2.Zero, Game.ScaleFactor * 2, SpriteEffects.None, 1);
                    DrawInventoryWindow(RenderStage.Sprites);
                    break;

                case WindowType.CraftingBench:
                    SpriteBatch.Draw(Crafting, new Vector2(
                        Game.GraphicsDevice.Viewport.Width / 2 - Scale(CraftingWindowRect.Width / 2),
                        Game.GraphicsDevice.Viewport.Height / 2 - Scale(CraftingWindowRect.Height / 2)),
                        CraftingWindowRect, Color.White, 0, Vector2.Zero, Game.ScaleFactor * 2, SpriteEffects.None, 1);
                    DrawCraftingWindow(RenderStage.Sprites);
                    break;

                case WindowType.Chest:
                    bool doubleChest = ((IChestWindow<ISlot>)Game.Client.CurrentWindow).DoubleChest;
                    Texture2D texture = (doubleChest ? _doubleChest : _chest);
                    Rectangle chestRect = (doubleChest ? _doubleChestWindowRect : _chestWindowRect);

                    SpriteBatch.Draw(texture,
                        new Vector2(Game.GraphicsDevice.Viewport.Width / 2 - Scale(chestRect.Width / 2),
                                    Game.GraphicsDevice.Viewport.Height / 2 - Scale(chestRect.Height / 2)),
                        chestRect, Color.White, 0, Vector2.Zero, Game.ScaleFactor * 2, SpriteEffects.None, 1);
                    DrawChestWindow(RenderStage.Sprites);
                    break;

                // TODO draw other window types
            }

            if (provider != null)
            {
                if (provider.GetIconTexture((byte)HeldItem.Metadata) != null)
                {
                    IconRenderer.RenderItemIcon(SpriteBatch, Items, provider,
                        (byte)HeldItem.Metadata, rect, Color.White);
                }
            }
            SpriteBatch.End();

            switch (Game.Client.CurrentWindow.Type)
            {
                case WindowType.Inventory:
                    DrawInventoryWindow(RenderStage.Models);
                    break;

                case WindowType.CraftingBench:
                    DrawCraftingWindow(RenderStage.Models);
                    break;

                case WindowType.Chest:
                    DrawChestWindow(RenderStage.Models);
                    break;

                // TODO draw other window types
            }
            if (provider != null)
            {
                if (provider.GetIconTexture((byte)HeldItem.Metadata) == null && provider is IBlockProvider)
                {
                    IconRenderer.RenderBlockIcon(Game, provider as IBlockProvider, (byte)HeldItem.Metadata, rect);
                }
            }

            SpriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.NonPremultiplied);
            switch (Game.Client.CurrentWindow.Type)
            {
                case WindowType.Inventory:
                    DrawInventoryWindow(RenderStage.Text);
                    break;

                case WindowType.CraftingBench:
                    DrawCraftingWindow(RenderStage.Text);
                    break;

                case WindowType.Chest:
                    DrawChestWindow(RenderStage.Text);
                    break;

                    // TODO draw other window types
            }
            if (provider != null)
            {
                if (HeldItem.Count > 1)
                {
                    int offset = 10;
                    if (HeldItem.Count >= 10)
                        offset -= 6;
                    mouse += new Point((int)Scale(offset), (int)Scale(5));
                    Font.DrawText(SpriteBatch, mouse.X, mouse.Y, HeldItem.Count.ToString(), Game.ScaleFactor);
                }
            }
            if (SelectedSlot >= 0)
            {
                var item = Game.Client.CurrentWindow[SelectedSlot];
                if (!item.Empty)
                {
                    var p = Game.ItemRepository.GetItemProvider(item.ID);
                    var size = Font.MeasureText(p.DisplayName);
                    mouse = Mouse.GetState().Position.ToVector2().ToPoint();
                    mouse += new Point(10, 10);
                    SpriteBatch.Draw(Game.White1x1, new Rectangle(mouse,
                        new Point(size.X + 10, size.Y + 15)),
                        new Color(Color.Black, 200));
                    Font.DrawText(SpriteBatch, mouse.X + 5, mouse.Y, p.DisplayName);
                }
            }
            SpriteBatch.End();
        }

        public override bool MouseMove(GameTime gameTime, MouseMoveEventArgs e)
        {
            if (Game.Client.CurrentWindow != null)
                return true;
            return base.MouseMove(gameTime, e);
        }

        private class MyHeldItem : IHeldItem
        {
            private readonly WindowModule _module;

            public MyHeldItem(WindowModule module)
            {
                _module = module;
            }

            public ItemStack HeldItem
            {
                get => _module.HeldItem;
                set { _module.HeldItem = value; }
            }
        }

        public override bool MouseButtonDown(GameTime gameTime, MouseButtonEventArgs e)
        {
            if (Game.Client.CurrentWindow == null)
                return false;
            var id = Game.Client.CurrentWindow.WindowID;
            if (id == -1)
                id = 0;
            var item = ItemStack.EmptyStack;

            if (SelectedSlot > -1)
                item = Game.Client.CurrentWindow[SelectedSlot];

            bool rightClick = (e.Button == MouseButton.Right);
            bool shiftClick = Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift);

            ActionConfirmation action;
            action = ((IClickHandler)Game.Client.CurrentWindow).HandleClick(SelectedSlot,
                rightClick, shiftClick, new MyHeldItem(this));
            if (object.ReferenceEquals(action, null))
                return true;
            ActionList.Add(action);

            IPacket packet = new ClickWindowPacket(id, SelectedSlot, rightClick,
                action.ActionNumber, shiftClick, item.ID, item.Count, item.Metadata);
            Game.Client.QueuePacket(packet);

            return true;
        }

        public override bool MouseButtonUp(GameTime gameTime, MouseButtonEventArgs e)
        {
            return Game.Client.CurrentWindow != null;
        }

        public override bool KeyDown(GameTime gameTime, KeyboardKeyEventArgs e)
        {
            if (Game.Client.CurrentWindow != null)
            {
                if (e.Key == Keys.Escape)
                {
                    // TODO When the player closes a window with a Crafting Grid,
                    //      any items in the Grid's input should be dropped.
                    if (Game.Client.CurrentWindow.Type != WindowType.Inventory)
                        Game.Client.QueuePacket(new CloseWindowPacket(Game.Client.CurrentWindow.WindowID));
                    Game.Client.CurrentWindow = null;
                    Mouse.SetPosition(Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2);
                    Game.ControlModule.IgnoreNextUpdate = true;
                }
                return true;
            }
            return base.KeyDown(gameTime, e);
        }

        private void DrawInventoryWindow(RenderStage stage)
        {
            // TODO fix hard-coded constants
            InventoryWindow window = (InventoryWindow)Game.Client.CurrentWindow;
            DrawWindowArea(window.CraftingGrid, InventoryWindow.CraftingOutputSlotIndex, 88, 26, InventoryWindowRect, stage);
            DrawWindowArea(window.Armor, InventoryWindow.ArmorSlotIndex, 8, 8, InventoryWindowRect, stage);
            DrawWindowArea(window.MainInventory, InventoryWindow.MainSlotIndex, 8, 84, InventoryWindowRect, stage);
            DrawWindowArea(window.Hotbar, InventoryWindow.HotbarSlotIndex, 8, 142, InventoryWindowRect, stage);
        }

        private void DrawCraftingWindow(RenderStage stage)
        {
            // TODO fix hard-coded constants
            CraftingBenchWindow window = (CraftingBenchWindow)Game.Client.CurrentWindow;
            DrawWindowArea(window.CraftingArea, CraftingBenchWindow.CraftingOutputSlotIndex, 29, 16, CraftingWindowRect, stage);
            DrawWindowArea(window.MainInventory, CraftingBenchWindow.MainSlotIndex, 8, 84, CraftingWindowRect, stage);
            DrawWindowArea(window.Hotbar, CraftingBenchWindow.HotbarSlotIndex, 8, 142, CraftingWindowRect, stage);
        }

        private void DrawChestWindow(RenderStage stage)
        {
            ChestWindow window = (ChestWindow)Game.Client.CurrentWindow;
            bool bSingleChest = !window.DoubleChest;
            Rectangle rect = bSingleChest ? _chestWindowRect : _doubleChestWindowRect;

            // TODO fix hard-coded constants
            //   8: number of pixels from the left of the Texture where the slot content starts
            //   18: number of pixels from the top of the Texture where the Chest slot content starts
            //   84: is the number of pixels from the top of the Texture where the Player's Inventory
            //       starts for a single chest.
            //   140: the number of pixesl from the top of the Texture where the Player's Inventory
            //       starts for a double chest
            //   58: the number of pixels from the top of the Player's Inventory to the top of the
            //       Hotbar inventory.
            int yPlayerInventory = bSingleChest ? 84 : 140;

            ISlots<ISlot> deleteme = window.ChestInventory;

            DrawWindowArea(window.ChestInventory, window.ChestSlotIndex, 8, 18, rect, stage);
            DrawWindowArea(window.MainInventory, window.MainSlotIndex, 8, yPlayerInventory, rect, stage);
            DrawWindowArea(window.Hotbar, window.HotbarSlotIndex, 8, yPlayerInventory + 58, rect, stage);
        }

        /// <summary>
        /// Draws the contents of one Window Area.
        /// </summary>
        /// <param name="area">The contents of this Window Area will be drawn.</param>
        /// <param name="startIndex"></param>
        /// <param name="xOffset">The x-coordinate within the GUI's Texture of the first slot of the Window Area.</param>
        /// <param name="yOffset">The r-coordinate within the GUI's Texture of the first slot of the Window Area.</param>
        /// <param name="frame">The Rectangle which is filled with the GUI Texture.</param>
        /// <param name="stage"></param>
        private void DrawWindowArea(ISlots<ISlot> area, int startIndex, int xOffset, int yOffset, Rectangle frame, RenderStage stage)
        {
            var mouse = Mouse.GetState().Position.ToVector2();
            var scale = new Point((int)(16 * Game.ScaleFactor * 2));
            var origin = new Point((int)(
                Game.GraphicsDevice.Viewport.Width / 2 - Scale(frame.Width / 2) + Scale(xOffset)),
                (int)(Game.GraphicsDevice.Viewport.Height / 2 - Scale(frame.Height / 2) + Scale(yOffset)));

            for (int i = 0; i < area.Count; i++)
            {
                ItemStack item = area[i].Item;
                int x = (int)((i % area.Width) * Scale(18));
                int y = (int)((i / area.Width) * Scale(18));
                if (area is ICraftingArea<ISlot>)
                {
                    // yes I know this is a crappy hack
                    if (i == 0)
                    {
                        if (area.Width == 2)
                        {
                            x = (int)Scale(144 - xOffset);
                            y = (int)Scale(36 - yOffset);
                        }
                        else
                        {
                            x = (int)Scale(124 - xOffset);
                            y = (int)Scale(35 - yOffset);
                        }
                    }
                    else
                    {
                        i--;
                        x = (int)((i % area.Width) * Scale(18));
                        y = (int)((i / area.Width) * Scale(18));
                        i++;
                    }
                }
                var position = origin + new Point(x, y);
                var rect = new Rectangle(position, scale);
                if (stage == RenderStage.Sprites && rect.Contains(mouse))
                {
                    SelectedSlot = (short)(startIndex + i);
                    SpriteBatch.Draw(Game.White1x1, rect, new Color(Color.White, 150));
                }

                if (item.Empty)
                    continue;

                var provider = Game.ItemRepository.GetItemProvider(item.ID);
                var texture = provider.GetIconTexture((byte)item.Metadata);
                if (texture != null && stage == RenderStage.Sprites)
                    IconRenderer.RenderItemIcon(SpriteBatch, Items, provider,
                        (byte)item.Metadata, rect, Color.White);

                if (texture == null && stage == RenderStage.Models && provider is IBlockProvider)
                    IconRenderer.RenderBlockIcon(Game, provider as IBlockProvider, (byte)item.Metadata, rect);

                if (stage == RenderStage.Text && item.Count > 1)
                {
                    int offset = 10;
                    if (item.Count >= 10)
                        offset -= 6;
                    position += new Point((int)Scale(offset), (int)Scale(5));
                    Font.DrawText(SpriteBatch, position.X, position.Y, item.Count.ToString(), Game.ScaleFactor);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            Game.IsMouseVisible = Game.Client.CurrentWindow != null;
            base.Update(gameTime);
        }

        private float Scale(float value)
        {
            return value * Game.ScaleFactor * 2;
        }
    }
}
