using System;
using TrueCraft.Core.World;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Server;
using fNbt;

namespace TrueCraft.Core.Logic
{
    public interface IBlockProvider : IItemProvider
    {
        byte ID { get; }
        double BlastResistance { get; }
        double Hardness { get; }
        byte Luminance { get; }
        bool Opaque { get; }
        bool RenderOpaque { get; }
        byte LightOpacity { get; }
        bool DiffuseSkyLight { get; }
        bool Flammable { get; }
        SoundEffectClass SoundEffect { get; }
        ToolMaterial EffectiveToolMaterials { get; }
        ToolType EffectiveTools { get; }
        BoundingBox? BoundingBox { get; } // NOTE: Will this eventually need to be metadata-aware?
        BoundingBox? InteractiveBoundingBox { get; } // NOTE: Will this eventually need to be metadata-aware?
        Tuple<int, int> GetTextureMap(byte metadata);
        void GenerateDropEntity(BlockDescriptor descriptor, IWorld world, IMultiplayerServer server, ItemStack heldItem);
        void BlockLeftClicked(BlockDescriptor descriptor, BlockFace face, IWorld world, IRemoteClient user);
        bool BlockRightClicked(BlockDescriptor descriptor, BlockFace face, IWorld world, IRemoteClient user);
        void BlockPlaced(BlockDescriptor descriptor, BlockFace face, IWorld world, IRemoteClient user);
        void BlockMined(BlockDescriptor descriptor, BlockFace face, IWorld world, IRemoteClient user);
        void BlockUpdate(BlockDescriptor descriptor, BlockDescriptor source, IMultiplayerServer server, IWorld world);
        void BlockLoadedFromChunk(GlobalVoxelCoordinates coords, IMultiplayerServer server, IWorld world);
        void TileEntityLoadedForClient(BlockDescriptor descriptor, IWorld world, NbtCompound compound, IRemoteClient client);
    }
}
