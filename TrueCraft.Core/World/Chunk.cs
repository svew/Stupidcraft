using System;
using System.Collections.Generic;
using fNbt;
using fNbt.Serialization;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Core.World
{
    public class Chunk : INbtSerializable, IChunk
    {
        private GlobalChunkCoordinates _coordinates;

        public const int Width = WorldConstants.ChunkWidth;
        public const int Height = WorldConstants.Height;
        public const int Depth = WorldConstants.ChunkDepth;

        public event EventHandler Disposed;

        #region Constructors
        public Chunk()
        {
            Biomes = new byte[Width * Depth];
            HeightMap = new int[Width * Depth];
            TileEntities = new Dictionary<LocalVoxelCoordinates, NbtCompound>();
            TerrainPopulated = false;
            LightPopulated = false;
            MaxHeight = 0;
            const int size = Width * Height * Depth;
            const int halfSize = size / 2;
            Data = new byte[size + halfSize * 3];
            Metadata = new NibbleSlice(Data, size, halfSize);
            BlockLight = new NibbleSlice(Data, size + halfSize, halfSize);
            SkyLight = new NibbleSlice(Data, size + halfSize * 2, halfSize);
        }

        public Chunk(GlobalChunkCoordinates coordinates) : this()
        {
            _coordinates = coordinates;
        }
        #endregion

        public void Dispose()
        {
            if (Disposed != null)
                Disposed(this, null);
        }

        [NbtIgnore]
        public DateTime LastAccessed { get; set; }
        [NbtIgnore]
        public bool IsModified { get; set; }
        [NbtIgnore]
        public byte[] Data { get; set; }
        [NbtIgnore]
        public NibbleSlice Metadata { get; set; }
        [NbtIgnore]
        public NibbleSlice BlockLight { get; set; }
        [NbtIgnore]
        public NibbleSlice SkyLight { get; set; }
        public byte[] Biomes { get; set; }
        public int[] HeightMap { get; set; }
        public int MaxHeight { get; private set; }

        /// <inheritdoc />
        [TagName("xPos")]
        public int X { get => _coordinates.X; }

        /// <inheritdoc />
        [TagName("zPos")]
        public int Z { get => _coordinates.Z; }

        /// <inheritdoc />
        public GlobalChunkCoordinates Coordinates { get => _coordinates; }

        [NbtIgnore]
        private bool _LightPopulated;
        public bool LightPopulated
        {
            get
            {
                return _LightPopulated;
            }
            set
            {
                _LightPopulated = value;
                IsModified = true;
            }
        }
        public Dictionary<LocalVoxelCoordinates, NbtCompound> TileEntities { get; }

        public long LastUpdate { get; set; }

        public bool TerrainPopulated { get; set; }

        [NbtIgnore]
        public IRegion ParentRegion { get; set; }

        public byte GetBlockID(LocalVoxelCoordinates coordinates)
        {
            int index = coordinates.Y + (coordinates.Z * Height) + (coordinates.X * Height * Width);
            return Data[index];
        }

        public byte GetMetadata(LocalVoxelCoordinates coordinates)
        {
            int index = coordinates.Y + (coordinates.Z * Height) + (coordinates.X * Height * Width);
            return Metadata[index];
        }

        public byte GetSkyLight(LocalVoxelCoordinates coordinates)
        {
            int index = coordinates.Y + (coordinates.Z * Height) + (coordinates.X * Height * Width);
            return SkyLight[index];
        }

        public byte GetBlockLight(LocalVoxelCoordinates coordinates)
        {
            int index = coordinates.Y + (coordinates.Z * Height) + (coordinates.X * Height * Width);
            return BlockLight[index];
        }

        /// <summary>
        /// Sets the block ID at specific coordinates relative to this chunk.
        /// Warning: The parent world's BlockChanged event handler does not get called.
        /// </summary>
        public void SetBlockID(LocalVoxelCoordinates coordinates, byte value)
        {
            IsModified = true;
            if (ParentRegion != null)
                ParentRegion.DamageChunk((LocalChunkCoordinates)Coordinates);
            int index = coordinates.Y + (coordinates.Z * Height) + (coordinates.X * Height * Width);
            Data[index] = value;
            if (value == AirBlock.BlockID)
                Metadata[index] = 0x0;
            var oldHeight = GetHeight((byte)coordinates.X, (byte)coordinates.Z);
            if (value == AirBlock.BlockID)
            {
                if (oldHeight <= coordinates.Y)
                {
                    // Shift height downwards
                    while (coordinates.Y > 0)
                    {
                        coordinates = new LocalVoxelCoordinates(coordinates.X, coordinates.Y - 1, coordinates.Z);
                        if (GetBlockID(coordinates) != AirBlock.BlockID)
                        {
                            SetHeight((byte)coordinates.X, (byte)coordinates.Z, coordinates.Y);
                            if (coordinates.Y > MaxHeight)
                                MaxHeight = coordinates.Y;
                            break;
                        }
                    }
                }
            }
            else
            {
                if (oldHeight < coordinates.Y)
                    SetHeight((byte)coordinates.X, (byte)coordinates.Z, coordinates.Y);
            }
        }

        /// <summary>
        /// Sets the metadata at specific coordinates relative to this chunk.
        /// Warning: The parent world's BlockChanged event handler does not get called.
        /// </summary>
        public void SetMetadata(LocalVoxelCoordinates coordinates, byte value)
        {
            IsModified = true;
            if (ParentRegion != null)
                ParentRegion.DamageChunk((LocalChunkCoordinates)Coordinates);
            int index = coordinates.Y + (coordinates.Z * Height) + (coordinates.X * Height * Width);
            Metadata[index] = value;
        }

        /// <summary>
        /// Sets the sky light at specific coordinates relative to this chunk.
        /// Warning: The parent world's BlockChanged event handler does not get called.
        /// </summary>
        public void SetSkyLight(LocalVoxelCoordinates coordinates, byte value)
        {
            IsModified = true;
            if (ParentRegion != null)
                ParentRegion.DamageChunk((LocalChunkCoordinates)Coordinates);
            int index = coordinates.Y + (coordinates.Z * Height) + (coordinates.X * Height * Width);
            SkyLight[index] = value;
        }

        /// <summary>
        /// Sets the block light at specific coordinates relative to this chunk.
        /// Warning: The parent world's BlockChanged event handler does not get called.
        /// </summary>
        public void SetBlockLight(LocalVoxelCoordinates coordinates, byte value)
        {
            IsModified = true;
            if (ParentRegion != null)
                ParentRegion.DamageChunk((LocalChunkCoordinates)Coordinates);
            int index = coordinates.Y + (coordinates.Z * Height) + (coordinates.X * Height * Width);
            BlockLight[index] = value;
        }
        
        /// <summary>
        /// Gets the tile entity for the given coordinates. May return null.
        /// </summary>
        public NbtCompound GetTileEntity(LocalVoxelCoordinates coordinates)
        {
            if (TileEntities.ContainsKey(coordinates))
                return TileEntities[coordinates];
            return null;
        }
        
        /// <summary>
        /// Sets the tile entity at the given coordinates to the given value.
        /// </summary>
        public void SetTileEntity(LocalVoxelCoordinates coordinates, NbtCompound value)
        {
            if (value == null && TileEntities.ContainsKey(coordinates))
                TileEntities.Remove(coordinates);
            else
                TileEntities[coordinates] = value;
            IsModified = true;
            if (ParentRegion != null)
                ParentRegion.DamageChunk((LocalChunkCoordinates)Coordinates);
        }

        /// <summary>
        /// Gets the height of the specified column.
        /// </summary>
        public int GetHeight(byte x, byte z)
        {
            return HeightMap[(x * Width) + z];
        }

        private void SetHeight(byte x, byte z, int value)
        {
            IsModified = true;
            HeightMap[(x * Width) + z] = value;
        }

        public void UpdateHeightMap()
        {
            for (byte x = 0; x < Chunk.Width; x++)
            {
                for (byte z = 0; z < Chunk.Depth; z++)
                {
                    int y;
                    for (y = Chunk.Height - 1; y >= 0; y--)
                    {
                        int index = y + (z * Height) + (x * Height * Width);
                        if (Data[index] != 0)
                        {
                            SetHeight(x, z, y);
                            if (y > MaxHeight)
                                MaxHeight = y;
                            break;
                        }
                    }
                    if (y == 0)
                        SetHeight(x, z, 0);
                }
            }
        }

        public NbtFile ToNbt()
        {
            var serializer = new NbtSerializer(typeof(Chunk));
            var compound = serializer.Serialize(this, "Level") as NbtCompound;
            var file = new NbtFile();
            file.RootTag.Add(compound);
            return file;
        }

        public static Chunk FromNbt(NbtFile nbt)
        {
            var serializer = new NbtSerializer(typeof(Chunk));
            var chunk = (Chunk)serializer.Deserialize(nbt.RootTag["Level"]);
            return chunk;
        }

        public NbtTag Serialize(string tagName)
        {
            var chunk = new NbtCompound(tagName);
            var entities = new NbtList("Entities", NbtTagType.Compound);
            chunk.Add(entities);
            chunk.Add(new NbtInt("X", X));
            chunk.Add(new NbtInt("Z", Z));
            chunk.Add(new NbtByte("LightPopulated", (byte)(LightPopulated ? 1 : 0)));
            chunk.Add(new NbtByte("TerrainPopulated", (byte)(TerrainPopulated ? 1 : 0)));
            chunk.Add(new NbtByteArray("Blocks", Data));
            chunk.Add(new NbtByteArray("Data", Metadata.ToArray()));
            chunk.Add(new NbtByteArray("SkyLight", SkyLight.ToArray()));
            chunk.Add(new NbtByteArray("BlockLight", BlockLight.ToArray()));
            
            var tiles = new NbtList("TileEntities", NbtTagType.Compound);
            foreach (var kvp in TileEntities)
            {
                var c = new NbtCompound();
                c.Add(new NbtList("coordinates", new[] { 
                    new NbtInt(kvp.Key.X),
                    new NbtInt(kvp.Key.Y),
                    new NbtInt(kvp.Key.Z)
                 }));
                 c.Add(new NbtList("value", new[] { kvp.Value }));
                 tiles.Add(c);
            }
            chunk.Add(tiles);
            
            // TODO: Entities
            return chunk;
        }

        public void Deserialize(NbtTag value)
        {
            var tag = (NbtCompound)value;

            int x = tag["X"].IntValue;
            int z = tag["Z"].IntValue;
            _coordinates = new GlobalChunkCoordinates(x, z);

            if (tag.Contains("TerrainPopulated"))
                TerrainPopulated = tag["TerrainPopulated"].ByteValue > 0;
            if (tag.Contains("LightPopulated"))
                LightPopulated = tag["LightPopulated"].ByteValue > 0;
            const int size = Width * Height * Depth;
            const int halfSize = size / 2;
            Data = new byte[(int)(size * 2.5)];
            Buffer.BlockCopy(tag["Blocks"].ByteArrayValue, 0, Data, 0, size);
            Metadata = new NibbleSlice(Data, size, halfSize);
            BlockLight = new NibbleSlice(Data, size + halfSize, halfSize);
            SkyLight = new NibbleSlice(Data, size + halfSize * 2, halfSize);
            
            Metadata.Deserialize(tag["Data"]);
            BlockLight.Deserialize(tag["BlockLight"]);
            SkyLight.Deserialize(tag["SkyLight"]);
            
            if (tag.Contains("TileEntities"))
            {
                foreach (var entity in tag["TileEntities"] as NbtList)
                {
                    TileEntities[new LocalVoxelCoordinates(entity["coordinates"][0].IntValue,
                        entity["coordinates"][1].IntValue,
                        entity["coordinates"][2].IntValue)] = entity["value"][0] as NbtCompound;
                }
            }
            UpdateHeightMap();

            // TODO: Entities
        }
    }
}
