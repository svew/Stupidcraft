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
        /// Acceleration Due To Gravity in metres per second squared (m/s^2)
        /// </summary>
        /// <remarks>
        /// Per: https://www.youtube.com/watch?v=RCeMKNeosJs
        /// </remarks>
        public static readonly double AccelerationDueToGravity = 32.656;

        /// <summary>
        /// Terminal Velocity in metres per second (m/s).
        /// </summary>
        /// <remarks>
        /// Per: https://www.youtube.com/watch?v=RCeMKNeosJs
        /// </remarks>
        public static readonly double TerminalVelocity = 10_000 / 319;

        /// <summary>
        /// The height a Player can jump, in metres.
        /// </summary>
        public static readonly double JumpHeight = 1.125;

        /// <summary>
        /// The initial vertical velocity when a Player jumps in metres per second (m/s).
        /// </summary>
        public static readonly double JumpVelocity = Math.Sqrt(2 * AccelerationDueToGravity * JumpHeight);

        /// <summary>
        /// A threshold difference - doubles with differences less than this
        /// may be treated as equal.
        /// </summary>
        public static readonly double Epsilon = 1e-8;
    }
}
