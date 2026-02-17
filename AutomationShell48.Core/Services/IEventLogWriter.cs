using System;

namespace AutomationShell48.Core.Services
{
    public interface IEventLogWriter
    {
        void WriteError(string message, Exception ex = null);
    }
}
