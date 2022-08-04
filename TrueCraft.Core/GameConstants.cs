using System;
namespace TrueCraft.Core
{
    public static class GameConstants
    {
        /// <summary>
        /// Number of Milliseconds per Game Tick.
        /// </summary>
        public static readonly int MillisecondsPerTick = 1000 / 20;

        /// <summary>
        /// The Player's walking speed in meters per second.
        /// </summary>
        public static readonly double WalkingSpeed = 4.3717;

        /// <summary>
        /// A threshold difference - doubles with differences less than this
        /// may be treated as equal.
        /// </summary>
        public static readonly double Epsilon = 1e-8;
    }
}
