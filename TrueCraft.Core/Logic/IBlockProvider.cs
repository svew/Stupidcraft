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
        void BlockLeftClicked(IServiceLocator serviceLocator, BlockDescriptor descriptor, BlockFace face, IDimension world, IRemoteClient user);
        bool BlockRightClicked(IServiceLocator serviceLocator, BlockDescriptor descriptor, BlockFace face, IDimension world, IRemoteClient user);
        void BlockPlaced(BlockDescriptor descriptor, BlockFace face, IDimension world, IRemoteClient user);
        void BlockMined(BlockDescriptor descriptor, BlockFace face, IDimension world, IRemoteClient user);
        void BlockUpdate(BlockDescriptor descriptor, BlockDescriptor source, IMultiplayerServer server, IDimension world);

        /// <summary>
        /// Called for each Block in a newly loaded Chunk.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="dimension">The Dimension containing the Block</param>
        /// <param name="coordinates">The coordinates of the Block within the Dimension.</param>
        void BlockLoadedFromChunk(IMultiplayerServer server, IDimension dimension, GlobalVoxelCoordinates coordinates);

        void TileEntityLoadedForClient(BlockDescriptor descriptor, IDimension world, NbtCompound compound, IRemoteClient client);
    }
}
