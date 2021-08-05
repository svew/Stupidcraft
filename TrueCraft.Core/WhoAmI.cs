using System;
namespace TrueCraft.Core
{
    public enum IAm
    {
        Unset = 0,
        Client = 1,
        Server = 2
    }

    public static class WhoAmI
    {
        private static IAm _whoami = IAm.Unset;

        /// <summary>
        /// Gets or sets whether the code is running client-side or server-side
        /// </summary>
        /// <remarks>
        /// This property can only be set once.  Set it at startup.
        /// </remarks>
        public static IAm Answer
        {
            get
            {
                if (_whoami == IAm.Unset)
                    throw new InvalidOperationException("WhoAmI.Answer not yet set.");
                return _whoami;
            }
            set
            {
                if (_whoami == value)
                    return;
                if (_whoami != IAm.Unset)
                    throw new InvalidOperationException("WhoAmI.Answer can only be set once.");
                if (value == IAm.Unset)
                    throw new ArgumentException("value cannot be set to IAm.Unset");
                _whoami = value;
            }
        }
    }
}
