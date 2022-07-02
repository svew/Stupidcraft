using System;

namespace TrueCraft.Core
{
    public class TrueCraftUser
    {
        public static string AuthServer = "https://truecraft.io";

        public TrueCraftUser()
        {
            Username = string.Empty;
            SessionId = string.Empty;
        }

        public string Username { get; set; }
        public string SessionId { get; set; }
    }
}