﻿using System;
using TrueCraft.API.World;

namespace TrueCraft.API.Logic
{
    public struct BlockDescriptor
    {
        public byte ID;
        public byte Metadata;
        public byte BlockLight;
        public byte SkyLight;
        // Optional
        public GlobalVoxelCoordinates Coordinates;
        // Optional
        public IChunk Chunk;
    }
}