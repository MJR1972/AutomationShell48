namespace AutomationShell48.Core.Services
{
    public interface IUserSettingsProvider
    {
        string Get(string key, string defaultValue);
        void Set(string key, string value);
        void Save();
    }
}
