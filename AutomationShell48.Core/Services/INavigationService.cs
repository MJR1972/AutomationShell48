using System;

namespace AutomationShell48.Core.Services
{
    public interface INavigationService
    {
        event Action<string> Navigated;
        void NavigateTo(string key);
    }
}
