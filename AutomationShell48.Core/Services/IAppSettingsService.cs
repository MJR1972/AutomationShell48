using AutomationShell48.Core.Theming;

namespace AutomationShell48.Core.Services
{
    public interface IAppSettingsService
    {
        ThemeKind LastTheme { get; set; }
        bool IsSidebarCollapsed { get; set; }
        string LastSelectedNavKey { get; set; }
        double WindowWidth { get; set; }
        double WindowHeight { get; set; }
        double WindowTop { get; set; }
        double WindowLeft { get; set; }
        bool HasSavedWindowBounds { get; set; }
        void Save();
    }
}
