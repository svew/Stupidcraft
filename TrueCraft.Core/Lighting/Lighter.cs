using System;
using TrueCraft.Core.Logic;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Lighting
{
    public abstract class Lighter : ILighter
    {
        protected readonly IDimension _dimension;

        protected readonly ILightingQueue _queue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dimension"></param>
        /// <param name="queue"></param>
        protected Lighter(IDimension dimension, ILightingQueue queue)
        {
            _dimension = dimension;
            _queue = queue;
        }

        /// <inheritdoc />
        public virtual void DoLightingOperation(LightingOperation operation)
        {
            switch(operation.Kind)
            {
                case LightingOperationKind.Initial:
                    DoInitialLightOperation((GlobalChunkCoordinates)operation.Seed);
                    break;

                case LightingOperationKind.Sky:
                    DoSkyLightOperation(operation.Seed, operation.Mode, operation.LightLevel);
                    break;

                case LightingOperationKind.Block:
                    if (operation.Mode == LightingOperationMode.Add)
                        DoAddBlockLightOperation(operation.Seed, operation.LightLevel);
                    else
                        DoSubtractBlockLightOperation(operation.Seed, operation.LightLevel);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="mode"></param>
        /// <param name="lightLevel"></param>
        private void DoSkyLightOperation(GlobalVoxelCoordinates seed, LightingOperationMode mode,
            byte lightLevel)
        {
            switch(mode)
            {
                case LightingOperationMode.Add:
                    DoAddSkyLightOperation(seed, lightLevel);
                    break;

                case LightingOperationMode.Subtract:
                    DoSubtractSkyLightOperation(seed, lightLevel);
                    break;

                case LightingOperationMode.BlockUpdate:
                    DoBlockUpdateSkyLightOperation(seed, lightLevel);
                    break;
            }
        }

        /// <summary>
        /// Handles adding a light of the given Light Level at the given Location. 
        /// </summary>
        /// <param name="lightLevel"></param>
        protected abstract void DoAddSkyLightOperation(GlobalVoxelCoordinates seed, byte lightLevel);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="lightLevel"></param>
        protected abstract void DoSubtractSkyLightOperation(GlobalVoxelCoordinates seed, byte lightLevel);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="lightLevel"></param>
        protected abstract void DoBlockUpdateSkyLightOperation(GlobalVoxelCoordinates seed, byte lightLevel);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="mode"></param>
        /// <param name="lightLevel"></param>
        private void DoBlockUpdateLightOperation(GlobalVoxelCoordinates seed, LightingOperationMode mode,
            byte lightLevel)
        {
            switch(mode)
            {
                case LightingOperationMode.Add:
                    DoAddBlockLightOperation(seed, lightLevel);
                    break;

                case LightingOperationMode.Subtract:
                    DoSubtractBlockLightOperation(seed, lightLevel);
                    break;

                case LightingOperationMode.BlockUpdate:
                    DoBlockUpdateBlockLightOperation(seed, lightLevel);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="lightLevel"></param>
        protected abstract void DoAddBlockLightOperation(GlobalVoxelCoordinates seed, byte lightLevel);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="lightLevel"></param>
        protected abstract void DoSubtractBlockLightOperation(GlobalVoxelCoordinates seed, byte lightLevel);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="lightLeve"></param>
        protected abstract void DoBlockUpdateBlockLightOperation(GlobalVoxelCoordinates seed, byte lightLeve);

        /// <summary>
        /// Performs a quick initial lighting of the specified Chunk.
        /// </summary>
        /// <param name="coordinates">The location of the Chunk to initially light.</param>
        protected abstract void DoInitialLightOperation(GlobalChunkCoordinates coordinates);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="lightLevel"></param>
        /// <param name="getter">A function delegate for retrieving the light level at specific coordinates.</param>
        /// <param name="setter">A delegate for setting the light level at specific coordinates.</param>
        protected void FloodFill(GlobalVoxelCoordinates seed, byte lightLevel,
            Func<GlobalVoxelCoordinates, byte> getter,
            Action<GlobalVoxelCoordinates, byte> setter)
        {
            // Picture a box with the seed at its centre.

            // xs, ys, and zs are the sizes of this box in the x-, y- and z-directions,
            // respectively.
            int xs = lightLevel * 2 - 1;
            int ys = xs;
            int zs = xs;

            // xo, yo, zo are the coordinates of the bottom northwest corner of the box with the
            // seed at its centre.
            int xo = seed.X - lightLevel + 1;
            int yo = seed.Y - lightLevel + 1;
            int zo = seed.Z - lightLevel + 1;

            // Truncate the box in the vertical direction at the limits of the dimension.
            if (yo < 0)
            {
                ys += yo;
                yo = 0;
            }
            if (yo + lightLevel > WorldConstants.Height)
            {
                ys -= yo + lightLevel - WorldConstants.Height;
            }

            Vector3i boxOffset = new Vector3i(xo, yo, zo);
            Bool3D finished = new Bool3D(xs, ys, zs, false);

            // Light the Seed
            setter(seed, lightLevel);

            // Flood fill the horizontal plane containing the seed, plus the
            // space above it.
            for (int y = 0; y < lightLevel; y++)
            {
                // Light around the voxel at/above the seed
                GlobalVoxelCoordinates curVoxel = new GlobalVoxelCoordinates(seed.X, seed.Y + y, seed.Z);
                LightAroundVoxel(curVoxel, getter, setter, finished, boxOffset);

                // Light voxels around the seed's column
                bool somethingGotLit = false;
                int sz = 1;
                if (sz < y - lightLevel)
                {
                    do
                    {
                        for (int x = -sz; x <= sz; x++)
                        {
                            curVoxel = new GlobalVoxelCoordinates(seed.X + x, seed.Y + y, seed.Z);
                            somethingGotLit |= LightAroundVoxel(curVoxel, getter, setter, finished, boxOffset);
                        }

                        for (int z = -sz + 1; z < sz; z++)
                        {
                            curVoxel = new GlobalVoxelCoordinates(seed.X, seed.Y + y, seed.Z + z);
                            somethingGotLit |= LightAroundVoxel(curVoxel, getter, setter, finished, boxOffset);
                        }

                        sz++;
                    } while (sz < y - lightLevel && somethingGotLit);
                }
            }

            // Flood fill below the seed's y-level.
            for (int y = -1; y > -lightLevel; y--)
            {
                // Light around the voxel below the seed
                GlobalVoxelCoordinates curVoxel = new GlobalVoxelCoordinates(seed.X, seed.Y + y, seed.Z);
                LightAroundVoxel(curVoxel, getter, setter, finished, boxOffset);

                // Light voxels around the seed's column
                bool somethingGotLit = false;
                int sz = 1;
                if (sz < y + lightLevel)
                {
                    do
                    {
                        for (int x = -sz; x <= sz; x++)
                        {
                            curVoxel = new GlobalVoxelCoordinates(seed.X + x, seed.Y + y, seed.Z);
                            somethingGotLit |= LightAroundVoxel(curVoxel, getter, setter, finished, boxOffset);
                        }

                        for (int z = -sz + 1; z < sz; z++)
                        {
                            curVoxel = new GlobalVoxelCoordinates(seed.X, seed.Y + y, seed.Z + z);
                            somethingGotLit |= LightAroundVoxel(curVoxel, getter, setter, finished, boxOffset);
                        }

                        sz++;
                    } while (sz < y + lightLevel && somethingGotLit);
                }
            }
        }

        /// <summary>
        /// Spreads the light from a Voxel to the neighbouring Voxels.
        /// </summary>
        /// <param name="coordinates">The Coordinates of the Voxel around which we are to set the lighting.</param>
        /// <param name="getter">A function delegate for retrieving the light level at specific coordinates.</param>
        /// <param name="setter">A delegate for setting the light level at specific coordinates.</param>
        /// <param name="finished"></param>
        /// <param name="offset"></param>
        /// <returns>true if at least one Voxel had its light level updated.</returns>
        protected bool LightAroundVoxel(GlobalVoxelCoordinates coordinates, 
            Func<GlobalVoxelCoordinates, byte> getter,
            Action<GlobalVoxelCoordinates, byte> setter, Bool3D finished,
            Vector3i offset)
        {
            bool rv = false;
            byte lightLevel = getter(coordinates);
            if (lightLevel > 1)
                foreach (Vector3i j in Vector3i.Neighbors6)
                {
                    GlobalVoxelCoordinates neighbour = coordinates + j;
                    if (!finished[neighbour.X - offset.X, neighbour.Y - offset.Y, neighbour.Z - offset.Z])
                    {
                        byte blockID = _dimension.GetBlockID(neighbour);
                        IBlockProvider provider = _dimension.BlockRepository.GetBlockProvider(blockID);
                        byte opacity = provider.LightOpacity;
                        if (opacity > lightLevel)
                            lightLevel = 0;
                        else
                            lightLevel -= Math.Max((byte)1, opacity);
                        LightVoxel(coordinates + j, lightLevel, getter, setter);
                        rv = true;
                    }
                }

            finished[coordinates.X + offset.X, coordinates.Y + offset.Y, coordinates.Z + offset.Z] = true;
            return rv;
        }

        /// <summary>
        /// Lights a single Voxel.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="lightLevel">The lightLevel to set</param>
        /// <param name="getter">A function delegate for retrieving the light level at specific coordinates.</param>
        /// <param name="setter">A delegate for setting the light level at specific coordinates.</param>
        /// <remarks>
        /// If the coordinates are outside the bounds of this Lighter's Dimension, the call
        /// is silently ignored.
        /// If the given light level is less than the existing light level, it will not be altered.
        /// </remarks>
        protected void LightVoxel(GlobalVoxelCoordinates coordinates, byte lightLevel,
            Func<GlobalVoxelCoordinates, byte> getter,
            Action<GlobalVoxelCoordinates, byte> setter)
        {
            // TODO better determination of "outside the dimension bounds"
            if (coordinates.Y < 0 || coordinates.Y >= WorldConstants.Height)
                return;
            if (getter(coordinates) > lightLevel)
                return;
            setter(coordinates, lightLevel);
        }

        /// <summary>
        /// Gets the Sky Light at the given coordinates.
        /// </summary>
        /// <param name="coordinates">The Coordinates from which to get the Sky Light</param>
        /// <returns>The Sky Light at the given coordinates.</returns>
        protected byte GetSkyLight(GlobalVoxelCoordinates coordinates)
        {
            return _dimension.GetSkyLight(coordinates);
        }

        /// <summary>
        /// Wrapper method for setting the Sky Light at the given location.
        /// </summary>
        /// <param name="coordinates">The location at which to set the Sky Light.</param>
        /// <param name="lightLevel"></param>
        protected void SetSkyLight(GlobalVoxelCoordinates coordinates, byte lightLevel)
        {
            _dimension.SetSkyLight(coordinates, lightLevel);
        }

        /// <summary>
        /// Gets the Block Light at the given coordinates.
        /// </summary>
        /// <param name="coordinates">The Coordinates from which to get the Block Light</param>
        /// <returns>The Block Light at the given coordinates.</returns>
        protected byte GetBlockLight(GlobalVoxelCoordinates coordinates)
        {
            return _dimension.GetBlockLight(coordinates);
        }

        /// <summary>
        /// Wrapper method for setting the Block Light at the given location.
        /// </summary>
        /// <param name="coordinates">The location at which to set the Block Light.</param>
        /// <param name="lightLevel"></param>
        protected void SetBlockLight(GlobalVoxelCoordinates coordinates, byte lightLevel)
        {
            _dimension.SetBlockLight(coordinates, lightLevel);
        }
    }
}
