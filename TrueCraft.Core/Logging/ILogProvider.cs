using System;

namespace TrueCraft.Core.Logging
{
    public interface ILogProvider
    {
        void Log(LogCategory category, string text, params object[] parameters);
    }
}
