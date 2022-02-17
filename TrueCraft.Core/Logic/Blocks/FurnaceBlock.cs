using System;
using System.Runtime.CompilerServices;
using TrueCraft.Core.Networking;
using TrueCraft.Core.World;
using TrueCraft.Core.Windows;
using System.Collections.Generic;
using fNbt;
using TrueCraft.Core.Server;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.Entities;
using TrueCraft.Core.Inventory;
using System.ComponentModel;

namespace TrueCraft.Core.Logic.Blocks
{
    public class FurnaceBlock : BlockProvider
    {
        protected class FurnaceState : IFurnaceSlots
        {
            private readonly NbtCompound _furnaceState;

            private const string BurnTimeRemainingTag = "BurnTime";

            private const string BurnTimeTotalTag = "BurnTotal";

            private const string CookTimeTag = "CookTime";

            private const int IngredientIndex = 0;

            private const int FuelIndex = 1;

            private const int OutputIndex = 2;

            /// <summary>
            /// Constructs a new default FurnaceState.
            /// </summary>
            public FurnaceState()
            {
                _furnaceState = new NbtCompound(new NbtTag[]
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
                IngredientSlot = new IngredientSlotImpl(IngredientIndex, this);
                FuelSlot = new FuelSlotImpl(FuelIndex, this);
                OutputSlot = new OutputSlotImpl(OutputIndex, this);
            }

            /// <summary>
            /// Constructs a new instance of FurnaceState from a Tile Entity.
            /// </summary>
            /// <param name="tileEntity">The Tile Entity which stores the furnace state on disk.</param>
            public FurnaceState(NbtCompound tileEntity)
            {
                _furnaceState = tileEntity;
                IngredientSlot = new IngredientSlotImpl(IngredientIndex, this);
                FuelSlot = new FuelSlotImpl(FuelIndex, this);
                OutputSlot = new OutputSlotImpl(OutputIndex, this);
            }

            private short InternalGet(string tag)
            {
                lock (_furnaceState)
                {
                    return (short)(_furnaceState.Get<NbtShort>(tag)?.Value ?? 0);
                }
            }

            private void InternalSet(string tag, short value)
            {
                lock (_furnaceState)
                {
                    NbtShort burnTime = _furnaceState.Get<NbtShort>(tag);
                    if (burnTime == null)
                    {
                        burnTime = new NbtShort(tag);
                        _furnaceState.Add(burnTime);
                    }
                    burnTime.Value = value;
                }
            }

            /// <summary>
            /// Gets or sets the current number of ticks remaining for the
            /// Furnace to be lit by the most recently consumed fuel item.
            /// </summary>
            public short BurnTimeRemaining
            {
                get => InternalGet(BurnTimeRemainingTag);
                set => InternalSet(BurnTimeRemainingTag, value);
            }

            /// <summary>
            /// Gets or sets the total number of ticks of burning provided by
            /// the most recently consumed fuel item.
            /// </summary>
            public short BurnTimeTotal
            {
                get => InternalGet(BurnTimeTotalTag);
                set => InternalSet(BurnTimeTotalTag, value);
            }

            /// <summary>
            /// Gets or sets the number of ticks spent cooking the current Ingredient.
            /// </summary>
            public short CookTime
            {
                get => InternalGet(CookTimeTag);
                set => InternalSet(CookTimeTag, value);
            }

            private ItemStack InternalSlotGet(int slotIndex)
            {
                lock(_furnaceState)
                {
                    NbtList items = _furnaceState.Get<NbtList>("Items");
                    return ItemStack.FromNbt(items.Get<NbtCompound>(slotIndex));
                }
            }

            private void InternalSlotSet(int slotIndex, ItemStack stack)
            {
                lock(_furnaceState)
                {
                    NbtList items = _furnaceState.Get<NbtList>("Items");
                    items[slotIndex] = stack.ToNbt();
                }
            }

            public ItemStack Ingredient
            {
                get => InternalSlotGet(IngredientIndex);
                set => InternalSlotSet(IngredientIndex, value);
            }

            public void DecrementIngredient()
            {
                InternalDecrement(IngredientIndex);
            }

            public ItemStack Fuel
            {
                get => InternalSlotGet(FuelIndex);
                set => InternalSlotSet(FuelIndex, value);
            }

            private void InternalDecrement(int slotIndex)
            {
                lock (_furnaceState)
                {
                    NbtList items = _furnaceState.Get<NbtList>("Items");
                    NbtCompound fuel = (NbtCompound)items[slotIndex];
                    NbtByte cnt = (NbtByte)fuel["Count"];
                    cnt.Value -= 1;
                }
            }

            public void DecrementFuel()
            {
                InternalDecrement(FuelIndex);
            }

            public ItemStack Output
            {
                get => InternalSlotGet(OutputIndex);
                set => InternalSlotSet(OutputIndex, value);
            }

            public void Save(IWorld world, GlobalVoxelCoordinates coordinates)
            {
                world.SetTileEntity(coordinates, _furnaceState);
            }

            #region IFurnaceSlots
            // TODO It is not possible to robustly implement the INotifyPropertyChanged
            //    as the NbtTag class does not implement it.  So we have no way to
            //    detect if the underlying storage is changed through other means.
            private abstract class SlotImpl : IServerSlot
            {
                private bool _dirty = false;

                protected FurnaceState _parent;

                protected SlotImpl(int slotIndex, FurnaceState parent)
                {
                    Index = slotIndex;
                    _parent = parent;
                }

                protected virtual void OnPropertyChanged([CallerMemberName]string name = "")
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                }

                public bool Dirty
                {
                    get => _dirty;
                    protected set
                    {
                        if (_dirty == value) return;
                        _dirty = value;
                        OnPropertyChanged();
                    }
                }

                public void SetClean()
                {
                    Dirty = false;
                }

                public int Index { get; }

                public abstract ItemStack Item { get; set; }

                public event PropertyChangedEventHandler PropertyChanged;

                public int CanAccept(ItemStack other)
                {
                    if (other.Empty) return 0;

                    ItemStack item = this.Item;

                    if (item.Empty) return other.Count;

                    if (item.CanMerge(other))
                    {
                        IItemProvider provider = ItemRepository.Get().GetItemProvider(item.ID);
                        int maxStack = provider.MaximumStack;
                        return Math.Min(maxStack - item.Count, other.Count);
                    }

                    return 0;
                }

                public SetSlotPacket GetSetSlotPacket(sbyte windowID)
                {
                    Dirty = false;
                    ItemStack item = Item;
                    return new SetSlotPacket(windowID, (short)Index, item.ID, item.Count, item.Metadata);
                }
            }

            private class IngredientSlotImpl : SlotImpl
            {

                public IngredientSlotImpl(int slotIndex, FurnaceState parent) : base(slotIndex, parent)
                {
                }

                public override ItemStack Item
                {
                    get => _parent.Ingredient;
                    set
                    {
                        _parent.Ingredient = value;
                        OnPropertyChanged();
                        Dirty = true;
                    }
                }
            }

            public IServerSlot IngredientSlot { get; }

            private class FuelSlotImpl : SlotImpl
            {
                public FuelSlotImpl(int slotIndex, FurnaceState parent) : base(slotIndex, parent)
                {

                }

                public override ItemStack Item
                {
                    get => _parent.Fuel;
                    set
                    {
                        _parent.Fuel = value;
                        OnPropertyChanged();
                        Dirty = true;
                    }
                }
            }

            public IServerSlot FuelSlot { get; }

            public IServerSlot OutputSlot { get; }

            private class OutputSlotImpl : SlotImpl
            {
                public OutputSlotImpl(int slotIndex, FurnaceState parent) : base (slotIndex, parent)
                {

                }

                public override ItemStack Item
                {
                    get => _parent.Output;
                    set
                    {
                        _parent.Output = value;
                        OnPropertyChanged();
                        Dirty = true;
                    }
                }
            }
            #endregion
        }

        protected class FurnaceEventSubject : IEventSubject
        {
            public event EventHandler Disposed;

            public void Dispose()
            {
                Disposed?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// A class to track the pair of the FurnaceWindow and its associated IRemoteClient instance
        /// </summary>
        protected class FurnaceWindowUser
        {
            public FurnaceWindowUser(IFurnaceWindow<IServerSlot> window, IRemoteClient user)
            {
                this.Window = window;
                this.User = user;
            }

            public IFurnaceWindow<IServerSlot> Window { get; }
            public IRemoteClient User { get; }
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

        // TODO: An instance of GlobalVoxelCoordinates is not sufficient.  If a
        //       furnace is placed in another dimension at the same coordinates,
        //       this will fail.
        protected static Dictionary<GlobalVoxelCoordinates, FurnaceEventSubject> _trackedFurnaces = new Dictionary<GlobalVoxelCoordinates, FurnaceEventSubject>();
        protected static Dictionary<GlobalVoxelCoordinates, List<FurnaceWindowUser>> _trackedFurnaceWindows = new Dictionary<GlobalVoxelCoordinates, List<FurnaceWindowUser>>();

        private FurnaceState GetState(IWorld world, GlobalVoxelCoordinates coords)
        {
            ServerOnly.Assert();

            NbtCompound tileEntity = world.GetTileEntity(coords);
            if (tileEntity == null)
            {
                FurnaceState rv =  new FurnaceState();
                rv.Save(world, coords);
                return rv;
            }

                return new FurnaceState(tileEntity);
        }

        private void UpdateWindows(GlobalVoxelCoordinates coords, FurnaceState state)
        {
            ServerOnly.Assert();

            if (_trackedFurnaceWindows.ContainsKey(coords))
            {
                Handling = true;
                foreach (var window in _trackedFurnaceWindows[coords])
                {
                    window.Window[0] = state.Ingredient;  // TODO hard-coded indices.
                    window.Window[1] = state.Fuel;
                    window.Window[2] = state.Output;

                    window.User.QueuePacket(new UpdateProgressPacket(
                        window.Window.WindowID, UpdateProgressPacket.ProgressTarget.ItemCompletion, state.CookTime));
                    var burnProgress = state.BurnTimeRemaining / (double)state.BurnTimeTotal;
                    var burn = (short)(burnProgress * 250);
                    window.User.QueuePacket(new UpdateProgressPacket(
                        window.Window.WindowID, UpdateProgressPacket.ProgressTarget.AvailableHeat, burn));
                }
                Handling = false;
            }
        }

        private void SetState(IWorld world, GlobalVoxelCoordinates coords, FurnaceState state)
        {
            state.Save(world, coords);
            UpdateWindows(coords, state);
        }

        public override void BlockLoadedFromChunk(GlobalVoxelCoordinates coords, IMultiplayerServer server, IWorld world)
        {
            ServerOnly.Assert();

            var state = GetState(world, coords);
            TryInitializeFurnace(state, server.Scheduler, world, coords, server.ItemRepository);
        }

        public override void BlockMined(BlockDescriptor descriptor, BlockFace face, IWorld world, IRemoteClient user)
        {
            ServerOnly.Assert();

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
            ServerOnly.Assert();

            FurnaceState state = GetState(world, descriptor.Coordinates);
            IInventoryFactory<IServerSlot> factory = new InventoryFactory<IServerSlot>();
            IFurnaceWindow<IServerSlot> window = factory.NewFurnaceWindow(user.Server.ItemRepository,
                SlotFactory<IServerSlot>.Get(), WindowIDs.GetWindowID(),
                state, user.Inventory, user.Hotbar,
                world, descriptor.Coordinates);

            user.OpenWindow(window);
            if (!_trackedFurnaceWindows.ContainsKey(descriptor.Coordinates))
                _trackedFurnaceWindows[descriptor.Coordinates] = new List<FurnaceWindowUser>();
            _trackedFurnaceWindows[descriptor.Coordinates].Add(new FurnaceWindowUser(window, user));
            window.WindowClosed += (sender, e) => _trackedFurnaceWindows.Remove(descriptor.Coordinates);

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
            if (_trackedFurnaces.ContainsKey(coords))
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
                _trackedFurnaces[coords] = subject;
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
                    state.DecrementFuel();
                    SetState(world, coords, state);
                    world.SetBlockID(coords, LitFurnaceBlock.BlockID);
                    var subject = new FurnaceEventSubject();
                    _trackedFurnaces[coords] = subject;
                    scheduler.ScheduleEvent("smelting", subject, TimeSpan.FromSeconds(1),
                        server => UpdateFurnace(server.Scheduler, world, coords, itemRepository));
                }
            }
        }

        private void UpdateFurnace(IEventScheduler scheduler, IWorld world, GlobalVoxelCoordinates coords, IItemRepository itemRepository)
        {
            // TODO: Why remove it on update?
            if (_trackedFurnaces.ContainsKey(coords))
                _trackedFurnaces.Remove(coords);

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
                    state.DecrementIngredient();
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