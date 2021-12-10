using System;

namespace TrueCraft.Core.Server
{
    // TODO: refactor to server-side only
    public static class WindowIDs
    {
        private static sbyte _curID = 0;

        public static sbyte GetWindowID()
        {
            _curID++;
            return _curID;
        }
    }
}
