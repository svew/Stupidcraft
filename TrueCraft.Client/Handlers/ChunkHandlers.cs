using System;
using MonoGame.Framework.Utilities.Deflate;
using TrueCraft.Client.Events;
using TrueCraft.Client.World;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.World;

namespace TrueCraft.Client.Handlers
{
    internal static class ChunkHandlers
    {
        public static void HandleBlockChange(IPacket _packet, MultiplayerClient client)
        {
            var packet = (BlockChangePacket)_packet;
            var coordinates = new GlobalVoxelCoordinates(packet.X, packet.Y, packet.Z);
            LocalVoxelCoordinates adjusted;
            IChunk? chunk;
            adjusted = client.Dimension.FindBlockPosition(coordinates, out chunk);
            // Relevant chunk is not loaded - ignore packet
            if (chunk is null)
                return;

            chunk.SetBlockID(adjusted, (byte)packet.BlockID);
            chunk.SetMetadata(adjusted, (byte)packet.Metadata);
            client.OnBlockChanged(new BlockChangeEventArgs(coordinates, new TrueCraft.Core.Logic.BlockDescriptor(),
                new TrueCraft.Core.Logic.BlockDescriptor()));
            client.OnChunkModified(new ChunkEventArgs(chunk));
        }

        public static void HandleChunkPreamble(IPacket _packet, MultiplayerClient client)
        {
        }

        public static void HandleChunkData(IPacket _packet, MultiplayerClient client)
        {
            ChunkDataPacket packet = (ChunkDataPacket)_packet;
            IChunk chunk;

            if (packet.Width == WorldConstants.ChunkWidth
                && packet.Height == WorldConstants.Height
                && packet.Depth == WorldConstants.ChunkDepth) // Fast path
            {
                chunk = new Chunk(packet);
                ((IDimensionClient)client.Dimension).AddChunk(chunk);
            }
            else // Slow path
            {
                // TODO: Is this code path ever used?  If so, will it ever be for a new-to-the-client
                //   chunk?  If so, this will generate a NullReferenceException.
                GlobalVoxelCoordinates coords = new GlobalVoxelCoordinates(packet.X, packet.Y, packet.Z);
                var data = ZlibStream.UncompressBuffer(packet.CompressedData);
                var adjustedCoords = client.Dimension.FindBlockPosition(coords, out chunk!);
                int x = adjustedCoords.X, y = adjustedCoords.Y, z = adjustedCoords.Z;
                int fullLength = packet.Width * packet.Height * packet.Depth; // Length of full sized byte section
                int nibbleLength = fullLength / 2; // Length of nibble sections
                for (int i = 0; i < fullLength; i++) // Iterate through block IDs
                {
                    chunk.SetBlockID(new LocalVoxelCoordinates(x, y, z), data[i]);
                    y++;
                    if (y >= packet.Height)
                    {
                        y = 0;
                        z++;
                        if (z >= packet.Depth)
                        {
                            z = 0;
                            x++;
                            if (x >= packet.Width)
                            {
                                x = 0;
                            }
                        }
                    }
                }
                x = adjustedCoords.X; y = adjustedCoords.Y; z = adjustedCoords.Z;
                for (int i = fullLength; i < nibbleLength; i++) // Iterate through metadata
                {
                    byte m = data[i];
                    chunk.SetMetadata(new LocalVoxelCoordinates(x, y, z), (byte)(m & 0xF));
                    chunk.SetMetadata(new LocalVoxelCoordinates(x, y + 1, z), (byte)(m & 0xF0 << 8));
                    y += 2;
                    if (y >= packet.Height)
                    {
                        y = 0;
                        z++;
                        if (z >= packet.Depth)
                        {
                            z = 0;
                            x++;
                            if (x >= packet.Width)
                            {
                                x = 0;
                            }
                        }
                    }
                }
                // TODO: Lighting
            }
            chunk.UpdateHeightMap();
            client.OnChunkLoaded(new ChunkEventArgs(chunk));
        }
    }
}