using System;
using TrueCraft.Core.Networking;
using TrueCraft.Core.World;
using TrueCraft.Core.Windows;
using System.Collections.Generic;
using fNbt;
using TrueCraft.Core.Server;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.Entities;

namespace TrueCraft.Core.Logic.Blocks
{
    public class FurnaceBlock : BlockProvider
    {
        protected class FurnaceState
        {
            /// <summary>
            /// Gets or sets the current number of ticks remaining for the
            /// Furnace to be lit by the most recently consumed fuel item.
            /// </summary>
            public short BurnTimeRemaining { get; set; }

            /// <summary>
            /// Gets or sets the total number of ticks of burning provided by
            /// the most recently consumed fuel item.
            /// </summary>
            public short BurnTimeTotal { get; set; }

            /// <summary>
            /// Gets or sets the number of ticks spent cooking the current Ingredient.
            /// </summary>
            public short CookTime { get; set; }

            public ItemStack Ingredient { get; set; }

            public ItemStack Fuel { get; set; }

            public ItemStack Output { get; set; }

            public NbtCompound ToNbt()
            {
                return new NbtCompound(new NbtTag[]
                    {
                        new NbtShort("BurnTime", BurnTimeRemaining),
                        new NbtShort("BurnTotal", BurnTimeTotal),
                        new NbtShort("CookTime", CookTime),
                        new NbtList("Items", new[]
                        {
                            Ingredient.ToNbt(),
                            Fuel.ToNbt(),
                            Output.ToNbt()
                        }, NbtTagType.Compound)
                    });
            }

            public FurnaceState(NbtCompound tileEntity)
            {
                NbtShort burnTime = tileEntity.Get<NbtShort>("BurnTime");
                NbtShort burnTotal = tileEntity.Get<NbtShort>("BurnTotal");
                NbtShort cookTime = tileEntity.Get<NbtShort>("CookTime");

                BurnTimeTotal = (short)(burnTotal?.Value ?? 0);
                BurnTimeRemaining = (short)(burnTime?.Value ?? 0);
                CookTime = (short)(cookTime?.Value ?? 200);

                NbtList items = tileEntity.Get<NbtList>("Items");
                int cnt = items?.Count ?? 0;
                if (cnt >= 3)
                {
                    Ingredient = ItemStack.FromNbt(items.Get<NbtCompound>(0));
                    Fuel = ItemStack.FromNbt(items.Get<NbtCompound>(1));
                    Output = ItemStack.FromNbt(items.Get<NbtCompound>(2));
                }
                else if (cnt == 2)
                {
                    Ingredient = ItemStack.FromNbt(items.Get<NbtCompound>(0));
                    Fuel = ItemStack.FromNbt(items.Get<NbtCompound>(1));
                    Output = ItemStack.EmptyStack;
                }
                else if (cnt == 1)
                {
                    Ingredient = ItemStack.FromNbt(items.Get<NbtCompound>(0));
                    Fuel = ItemStack.EmptyStack;
                    Output = ItemStack.EmptyStack;
                }
                else
                {
                    Ingredient = ItemStack.EmptyStack;
                    Fuel = ItemStack.EmptyStack;
                    Output = ItemStack.EmptyStack;
                }
            }
        }

        protected class FurnaceEventSubject : IEventSubject
        {
            public event EventHandler Disposed;

            public void Dispose()
            {
                // TODO: if anything subscribes to the Disposed event, we'll get infinite recursion...
                if (Disposed != null)
                    Dispose();
            }
        }

        public static readonly byte BlockID = 0x3D;

        public override byte ID { get { return 0x3D; } }

        public override double BlastResistance { get { return 17.5; } }

        public override double Hardness { get { return 3.5; } }

        public override byte Luminance { get { return 0; } }

        public override string DisplayName { get { return "Furnace"; } }

        public override ToolType EffectiveTools
        {
            get
            {
                return ToolType.Pickaxe;
            }
        }

        protected override ItemStack[] GetDrop(BlockDescriptor descriptor, ItemStack item)
        {
            return new[] { new ItemStack(BlockID) };
        }

        protected static Dictionary<GlobalVoxelCoordinates, FurnaceEventSubject> TrackedFurnaces { get; set; }
        protected static Dictionary<GlobalVoxelCoordinates, List<IWindowContent>> TrackedFurnaceWindows { get; set; }

        public FurnaceBlock()
        {
            // TODO: Why are static members initialized in an instance constructor???
            TrackedFurnaces = new Dictionary<GlobalVoxelCoordinates, FurnaceEventSubject>();
            TrackedFurnaceWindows = new Dictionary<GlobalVoxelCoordinates, List<IWindowContent>>();
        }

        private NbtCompound CreateTileEntity()
        {
            return new NbtCompound(new NbtTag[]
            {
                new NbtShort("BurnTime", 0),
                new NbtShort("BurnTotal", 0),
                new NbtShort("CookTime", -1),
                new NbtList("Items", new[]
                {
                    ItemStack.EmptyStack.ToNbt(),
                    ItemStack.EmptyStack.ToNbt(),
                    ItemStack.EmptyStack.ToNbt()
                }, NbtTagType.Compound)
            });
        }

        private FurnaceState GetState(IWorld world, GlobalVoxelCoordinates coords)
        {
#if DEBUG
            if (WhoAmI.Answer == IAm.Client)
                throw new ApplicationException(Strings.SERVER_CODE_ON_CLIENT);
#endif
            NbtCompound tileEntity = world.GetTileEntity(coords) ?? CreateTileEntity();

            return new FurnaceState(tileEntity);
        }

        private void UpdateWindows(GlobalVoxelCoordinates coords, FurnaceState state)
        {
            if (TrackedFurnaceWindows.ContainsKey(coords))
            {
                Handling = true;
                foreach (var window in TrackedFurnaceWindows[coords])
                {
                    window[0] = state.Ingredient;  // TODO hard-coded indices.
                    window[1] = state.Fuel;
                    window[2] = state.Output;

                    window.Client.QueuePacket(new UpdateProgressPacket(
                        window.ID, UpdateProgressPacket.ProgressTarget.ItemCompletion, state.CookTime));
                    var burnProgress = state.BurnTimeRemaining / (double)state.BurnTimeTotal;
                    var burn = (short)(burnProgress * 250);
                    window.Client.QueuePacket(new UpdateProgressPacket(
                        window.ID, UpdateProgressPacket.ProgressTarget.AvailableHeat, burn));
                }
                Handling = false;
            }
        }

        private void SetState(IWorld world, GlobalVoxelCoordinates coords, FurnaceState state)
        {
            world.SetTileEntity(coords, state.ToNbt());
            UpdateWindows(coords, state);
        }

        public override void BlockLoadedFromChunk(GlobalVoxelCoordinates coords, IMultiplayerServer server, IWorld world)
        {
            var state = GetState(world, coords);
            TryInitializeFurnace(state, server.Scheduler, world, coords, server.ItemRepository);
        }

        public override void BlockMined(BlockDescriptor descriptor, BlockFace face, IWorld world, IRemoteClient user)
        {
            var entity = world.GetTileEntity(descriptor.Coordinates);
            if (entity != null)
            {
                foreach (var item in (NbtList)entity["Items"])
                {
                    var manager = user.Server.GetEntityManagerForWorld(world);
                    var slot = ItemStack.FromNbt((NbtCompound)item);
                    manager.SpawnEntity(new ItemEntity(new Vector3(descriptor.Coordinates.X + 0.5, descriptor.Coordinates.Y + 0.5, descriptor.Coordinates.Z + 0.5), slot));
                }
                world.SetTileEntity(descriptor.Coordinates, null);
            }
            base.BlockMined(descriptor, face, world, user);
        }

        public override bool BlockRightClicked(BlockDescriptor descriptor, BlockFace face, IWorld world, IRemoteClient user)
        {
#if DEBUG
            if (WhoAmI.Answer == IAm.Client)
                throw new ApplicationException(Strings.SERVER_CODE_ON_CLIENT);
#endif
            WindowContentFactory factory = new WindowContentFactory();
            IWindowContent window = factory.NewFurnaceWindowContent(user.Inventory, user.Hotbar,
                             descriptor.Coordinates,
                             user.Server.ItemRepository);

            user.OpenWindow(window);
            if (!TrackedFurnaceWindows.ContainsKey(descriptor.Coordinates))
                TrackedFurnaceWindows[descriptor.Coordinates] = new List<IWindowContent>();
            TrackedFurnaceWindows[descriptor.Coordinates].Add(window);
            window.Disposed += (sender, e) => TrackedFurnaceWindows.Remove(descriptor.Coordinates);

            FurnaceState state = GetState(world, descriptor.Coordinates);
            UpdateWindows(descriptor.Coordinates, state);

            // TODO: Set window progress appropriately

            //window.WindowChange += (sender, e) => FurnaceWindowChanged(sender, e, world);
            return false;
        }

        private bool Handling = false;

        // TODO Move to OnWindowChanged - separate into Client- and Server-side methods.
        //protected void FurnaceWindowChanged(object sender, WindowChangeEventArgs e, IWorld world)
        //{
        //    if (Handling)
        //        return;
        //    IFurnaceWindowContent window = sender as IFurnaceWindowContent;
        //    int index = e.SlotIndex;
        //    if (window.IsPlayerInventorySlot(index))
        //        return;

        //    Handling = true;
        //    e.Handled = true;
        //    window[index] = e.Value;

        //    var state = GetState(world, window.Coordinates);

        //    state.Ingredient = window[0];  // TODO hard-coded indices
        //    state.Fuel = window[1];
        //    state.Output = window[2];

        //    SetState(world, window.Coordinates, state);

        //    Handling = true;

        //    if (!TrackedFurnaces.ContainsKey(window.Coordinates))
        //    {
        //        // Set up the initial state
        //        TryInitializeFurnace(state, window.EventScheduler, world, window.Coordinates, window.ItemRepository);
        //    }

        //    Handling = false;
        //}

        private void TryInitializeFurnace(FurnaceState state, IEventScheduler scheduler, IWorld world,
                                          GlobalVoxelCoordinates coords, IItemRepository itemRepository)
        {
            if (TrackedFurnaces.ContainsKey(coords))
                return;

            ItemStack inputStack = state.Ingredient;
            ItemStack fuelStack = state.Fuel;
            ItemStack outputStack = state.Output;

            var input = itemRepository.GetItemProvider(inputStack.ID) as ISmeltableItem;
            var fuel = itemRepository.GetItemProvider(fuelStack.ID) as IBurnableItem;

            if (state.BurnTimeRemaining > 0)
            {
                if (state.CookTime == -1 && input != null && (outputStack.Empty || outputStack.CanMerge(input.SmeltingOutput)))
                {
                    state.CookTime = 0;
                    SetState(world, coords, state);
                }
                var subject = new FurnaceEventSubject();
                TrackedFurnaces[coords] = subject;
                scheduler.ScheduleEvent("smelting", subject, TimeSpan.FromSeconds(1),
                    server => UpdateFurnace(server.Scheduler, world, coords, itemRepository));
                return;
            }

            if (fuel != null && input != null) // We can maybe start
            {
                if (outputStack.Empty || outputStack.CanMerge(input.SmeltingOutput))
                {
                    // We can definitely start
                    state.BurnTimeRemaining = state.BurnTimeTotal = (short)(fuel.BurnTime.TotalSeconds * 20);  // TODO Hard-coded constant for 20 ticks per second.
                    state.CookTime = 0;
                    state.Fuel = state.Fuel.GetReducedStack(1);
                    SetState(world, coords, state);
                    world.SetBlockID(coords, LitFurnaceBlock.BlockID);
                    var subject = new FurnaceEventSubject();
                    TrackedFurnaces[coords] = subject;
                    scheduler.ScheduleEvent("smelting", subject, TimeSpan.FromSeconds(1),
                        server => UpdateFurnace(server.Scheduler, world, coords, itemRepository));
                }
            }
        }

        private void UpdateFurnace(IEventScheduler scheduler, IWorld world, GlobalVoxelCoordinates coords, IItemRepository itemRepository)
        {
            // TODO: Why remove it on update?
            if (TrackedFurnaces.ContainsKey(coords))
                TrackedFurnaces.Remove(coords);

            if (world.GetBlockID(coords) != FurnaceBlock.BlockID && world.GetBlockID(coords) != LitFurnaceBlock.BlockID)
            {
                /*if (window != null && !window.IsDisposed)
                    window.Dispose();*/
                return;
            }

            var state = GetState(world, coords);

            var inputStack = state.Ingredient;
            var outputStack = state.Output;

            var input = itemRepository.GetItemProvider(inputStack.ID) as ISmeltableItem;

            // Update burn time
            var burnTime = state.BurnTimeRemaining;
            if (state.BurnTimeRemaining > 0)
            {
                state.BurnTimeRemaining -= 20; // ticks
                if (state.BurnTimeRemaining <= 0)
                {
                    state.BurnTimeRemaining = 0;
                    state.BurnTimeTotal = 0;
                    world.SetBlockID(coords, FurnaceBlock.BlockID);
                }
            }

            // Update cook time
            if (state.CookTime < 200 && state.CookTime >= 0)
            {
                state.CookTime += 20; // ticks
                if (state.CookTime >= 200)
                    state.CookTime = 200;
            }

            // Are we done cooking?
            if (state.CookTime == 200 && burnTime > 0)
            {
                state.CookTime = -1;
                if (input != null && (outputStack.Empty || outputStack.CanMerge(input.SmeltingOutput)))
                {
                    if (outputStack.Empty)
                        outputStack = input.SmeltingOutput;
                    else if (outputStack.CanMerge(input.SmeltingOutput))
                        outputStack.Count += input.SmeltingOutput.Count;
                    state.Output = outputStack;
                    state.Ingredient = state.Ingredient.GetReducedStack(1);
                }
            }

            SetState(world, coords, state);
            TryInitializeFurnace(state, scheduler, world, coords, itemRepository);
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(13, 2);
        }

        public override void BlockPlaced(BlockDescriptor descriptor, BlockFace face, IWorld world, IRemoteClient user)
        {
            world.SetMetadata(descriptor.Coordinates, (byte)MathHelper.DirectionByRotationFlat(user.Entity.Yaw, true));
        }
    }

    public class LitFurnaceBlock : FurnaceBlock
    {
        public static readonly new byte BlockID = 0x3E;

        public override byte ID { get { return 0x3E; } }

        public override byte Luminance { get { return 13; } }

        public override bool Opaque { get { return false; } }

        public override string DisplayName { get { return "Furnace (lit)"; } }
    }
}