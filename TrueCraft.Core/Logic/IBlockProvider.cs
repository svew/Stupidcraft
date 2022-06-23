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
        Tuple<int, int>? GetTextureMap(byte metadata);
        void GenerateDropEntity(BlockDescriptor descriptor, IDimension world, IMultiplayerServer server, ItemStack heldItem);
        void BlockLeftClicked(BlockDescriptor descriptor, BlockFace face, IDimension world, IRemoteClient user);
        bool BlockRightClicked(BlockDescriptor descriptor, BlockFace face, IDimension world, IRemoteClient user);
        void BlockPlaced(BlockDescriptor descriptor, BlockFace face, IDimension world, IRemoteClient user);
        void BlockMined(BlockDescriptor descriptor, BlockFace face, IDimension world, IRemoteClient user);
        void BlockUpdate(BlockDescriptor descriptor, BlockDescriptor source, IMultiplayerServer server, IDimension world);
        void BlockLoadedFromChunk(GlobalVoxelCoordinates coords, IMultiplayerServer server, IDimension world);
        void TileEntityLoadedForClient(BlockDescriptor descriptor, IDimension world, NbtCompound compound, IRemoteClient client);
    }
}
