using AutomationShell48.Core.Theming;

namespace AutomationShell48.Core.Services
{
    public interface IThemeService
    {
        ThemeKind CurrentTheme { get; }
        void ApplyTheme(ThemeKind kind);
        void ToggleTheme();
    }
}
