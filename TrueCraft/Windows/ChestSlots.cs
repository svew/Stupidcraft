using System;
using fNbt;
using TrueCraft.API;
using TrueCraft.API.World;
using TrueCraft.Core.Windows;

namespace TrueCraft.Windows
{
    public class ChestSlots : Slots
    {
        private readonly IWorld _world;

        private readonly GlobalVoxelCoordinates _chest;

        private readonly GlobalVoxelCoordinates _otherHalf;

        /// <summary>
        /// Constructs a set of Slots representing the content of the Chest
        /// </summary>
        /// <param name="world">The World in which the Chest is located.</param>
        /// <param name="chest">The location of the Chest.</param>
        /// <param name="otherHalf">If this is a Double Chest, the location of the other half of the Chest; otherwise null.</param>
        public ChestSlots(IWorld world,
            GlobalVoxelCoordinates chest,
            GlobalVoxelCoordinates otherHalf) :
            base((otherHalf is not null ? 2 : 1) * ChestWindowConstants.ChestLength,
                ChestWindowConstants.ChestWidth,
                (otherHalf is not null ? 2 : 1) * ChestWindowConstants.ChestHeight)
        {
#if DEBUG
            if (world is null)
                throw new ArgumentNullException($"{nameof(world)} must not be null.");

            if (otherHalf is not null)
            {
                if (otherHalf.Y != chest.Y)
                    throw new ArgumentException("The other half of the chest must be at the same Y-Level.");
                int diff = Math.Abs(otherHalf.Z - chest.Z) + Math.Abs(otherHalf.X - chest.X);
                if (diff != 1)
                    throw new ArgumentException("The other half of the chest must be adjacent.");
            }
#endif

            _world = world;

            if (otherHalf is null)
            {
                _chest = chest;
                _otherHalf = otherHalf;
            }
            else
            {
                if (otherHalf.X < chest.X || otherHalf.Z < chest.Z)
                {
                    _chest = otherHalf;
                    _otherHalf = chest;
                }
                else
                {
                    _chest = chest;
                    _otherHalf = otherHalf;
                }
            }

            Load();
        }

        public override ItemStack this[int index]
        {
            get => base[index];
            set
            {
                if (base[index] == value)
                    return;

                base[index] = value;
                Save(index);
            }
        }

        private void Load()
        {
            Load(_chest, 0);
            if (_otherHalf is not null)
                Load(_otherHalf, ChestWindowConstants.ChestLength);
        }

        private void Load(GlobalVoxelCoordinates location, int offset)
        {
            NbtCompound entity = _world.GetTileEntity(location);
            if (entity is null)
                return;

            NbtList items = (NbtList)entity["Items"];

            foreach (NbtCompound item in items)
            {
                ItemStack stack = ItemStack.FromNbt(item);
                base[stack.Index + offset] = stack;
            }
        }

        private void Save(int index)
        {
            NbtList entity = new NbtList("Items", NbtTagType.Compound);

            int offset;
            GlobalVoxelCoordinates location;
            if (index < ChestWindowConstants.ChestLength)
            {
                offset = 0;
                location = _chest;
            }
            else
            {
                offset = ChestWindowConstants.ChestLength;
                location = _otherHalf;
            }

            for (int i = 0; i < ChestWindowConstants.ChestLength; i++)
            {
                ItemStack item = base[i + offset];
                if (!item.Empty)
                {
                    item.Index = i;
                    entity.Add(item.ToNbt());
                }
            }

            NbtCompound newEntity = _world.GetTileEntity(location);
            if (newEntity is null)
                newEntity = new NbtCompound(new[] { entity });
            else
                newEntity["Items"] = entity;
            _world.SetTileEntity(location, newEntity);
        }
    }
}
