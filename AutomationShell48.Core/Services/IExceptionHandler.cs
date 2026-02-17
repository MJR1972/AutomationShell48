using System;

namespace AutomationShell48.Core.Services
{
    public interface IExceptionHandler
    {
        void Handle(Exception ex, string context = null, bool isTerminating = false);
    }
}
