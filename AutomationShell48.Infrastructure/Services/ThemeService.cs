using AutomationShell48.Core.Services;
using AutomationShell48.Core.Theming;

namespace AutomationShell48.Infrastructure.Services
{
    public class ThemeService : IThemeService
    {
        private readonly IThemeResourceManager _resourceManager;

        public ThemeService(IThemeResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
            CurrentTheme = ThemeKind.Light;
        }

        public ThemeKind CurrentTheme { get; private set; }

        public void ApplyTheme(ThemeKind kind)
        {
            CurrentTheme = kind;
            _resourceManager?.ApplyThemeResources(kind);
        }

        public void ToggleTheme()
        {
            ApplyTheme(CurrentTheme == ThemeKind.Light ? ThemeKind.Dark : ThemeKind.Light);
        }
    }
}
