using System;
using System.Diagnostics;

namespace TrueCraft.Core.Server
{
    public static class ServerOnly
    {
        /// <summary>
        /// 
        /// </summary>
        [Conditional("DEBUG")]
        public static void Assert()
        {
            if (WhoAmI.Answer != IAm.Server)
                throw new ApplicationException(Strings.SERVER_CODE_ON_CLIENT);
        }
    }
}
