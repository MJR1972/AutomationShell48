using System;
using AutomationShell48.Core.Services;

namespace AutomationShell48.Infrastructure.Services
{
    public class NavigationService : INavigationService
    {
        public event Action<string> Navigated;

        public void NavigateTo(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            Navigated?.Invoke(key);
        }
    }
}
