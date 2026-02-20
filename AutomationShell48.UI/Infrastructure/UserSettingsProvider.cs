using AutomationShell48.Core.Services;
using AutomationShell48.UI.Properties;

namespace AutomationShell48.UI.Infrastructure
{
    public class UserSettingsProvider : IUserSettingsProvider
    {
        public string Get(string key, string defaultValue)
        {
            var value = Settings.Default[key];
            return value == null ? defaultValue : value.ToString();
        }

        public void Set(string key, string value)
        {
            Settings.Default[key] = value;
        }

        public void Save()
        {
            Settings.Default.Save();
        }
    }
}

