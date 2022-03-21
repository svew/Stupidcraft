using System;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.World;

namespace TrueCraft.TerrainGen.Decorations
{
    public class Dungeon : Decoration
    {
        Vector3 Size = new Vector3(7, 5, 7);

        const int MaxEntrances = 5;

        public override bool ValidLocation(LocalVoxelCoordinates location)
        {
            var OffsetSize = Size + new Vector3(1, 1, 1);
            if (location.X + (int)OffsetSize.X >= Chunk.Width
                || location.Z + (int)OffsetSize.Z >= Chunk.Depth
                || location.Y + (int)OffsetSize.Y >= Chunk.Height)
                return false;
            return true;
        }

        public override bool GenerateAt(int seed, IChunk chunk, LocalVoxelCoordinates location)
        {
            Console.WriteLine("Dungeon in chunk {0}", chunk.Coordinates);
            if (!ValidLocation(location))
                return false;

            Random random = new Random(seed);

            //Generate room
            GenerateCuboid(chunk, location, Size, CobblestoneBlock.BlockID, 0x0, 0x2);

            //Randomly add mossy cobblestone to floor
            MossFloor(chunk, location, random);

            //Place Spawner
            int spawnerX = (int)(location.X + ((Size.X + 1) / 2));
            int spawnerY = location.Y + 1;
            int spawnerZ = (int)(location.Z + ((Size.Z + 1) / 2));
            chunk.SetBlockID(new LocalVoxelCoordinates(spawnerX, spawnerY, spawnerZ), MonsterSpawnerBlock.BlockID);
            
            //Create entrances
            CreateEntrances(chunk, location, random);

            //Place Chests
            PlaceChests(chunk, location, random);

            return true;
        }

        private void CreateEntrances(IChunk chunk, LocalVoxelCoordinates location, Random random)
        {
            int entrances = 0;

            // TODO? Optimize by running around the outside of the Dungeon rather
            //    than through all the interior blocks that cannot be entrances anyway
            //     This would take the algorithm from N**2 to 4*N
            // Optimize by moving the check on X & Z to the loop limits.
            // Note that the above would have the effect of altering the number of
            // calls to random, and therefore the generated world would be different.

            for (int X = location.X; X < location.X + Size.X; X++)
            {
                if (entrances >= MaxEntrances)
                    break;
                for (int Z = location.Z; Z < location.Z + Size.Z; Z++)
                {
                    if (entrances >= MaxEntrances)
                        break;
                    if (random.Next(0, 3) == 0 && IsCuboidWall(new LocalVoxelCoordinates(X, 0, Z), location, Size)
                        && !IsCuboidCorner(new LocalVoxelCoordinates(X, 0, Z), location, Size))
                    {
                        var blockLocation = new LocalVoxelCoordinates(X, location.Y + 1, Z);
                        if (blockLocation.X < 0 || blockLocation.X >= Chunk.Width
                            || blockLocation.Z < 0 || blockLocation.Z >= Chunk.Depth
                            || blockLocation.Y < 0 || blockLocation.Y >= Chunk.Height)
                            continue;
                        chunk.SetBlockID(blockLocation, AirBlock.BlockID);
                        chunk.SetBlockID(new LocalVoxelCoordinates(X, location.Y + 2, Z), AirBlock.BlockID);
                        entrances++;
                    }
                }
            }
        }

        private void MossFloor(IChunk chunk, LocalVoxelCoordinates location, Random random)
        {
            for (int x = location.X; x < location.X + Size.X; x++)
            {
                for (int z = location.Z; z < location.Z + Size.Z; z++)
                {
                    if (x < 0 || x >= Chunk.Width
                            || z < 0 || z >= Chunk.Depth
                            || location.Y < 0 || location.Y >= Chunk.Height)
                            continue;
                    if (random.Next(0, 3) == 0)
                        chunk.SetBlockID(new LocalVoxelCoordinates(x, location.Y, z), MossStoneBlock.BlockID);
                }
            }
        }

        private void PlaceChests(IChunk chunk, LocalVoxelCoordinates location, Random random)
        {
            int aboveY = location.Y + 1;
            var chests = random.Next(0, 2);
            for (int i = 0; i < chests; i++)
            {
                for (int attempts = 0; attempts < 10; attempts++)
                {
                    var x = random.Next(location.X, location.X + (int)Size.X);
                    var z = random.Next(location.Z, location.Z + (int)Size.Z);
                    if (!IsCuboidWall(new LocalVoxelCoordinates(x, 0, z), location, Size) && !IsCuboidCorner(new LocalVoxelCoordinates(x, 0, z), location, Size))
                    {
                        if (NeighboursBlock(chunk, new LocalVoxelCoordinates(x, aboveY, z), CobblestoneBlock.BlockID))
                        {
                            if (x < 0 || x >= Chunk.Width
                                || z < 0 || z >= Chunk.Depth
                                || aboveY < 0 || aboveY >= Chunk.Height)
                            continue;
                            chunk.SetBlockID(new LocalVoxelCoordinates(x, aboveY, z), ChestBlock.BlockID);
                            break;
                        }
                    }
                }
            }
        }
    }
}
