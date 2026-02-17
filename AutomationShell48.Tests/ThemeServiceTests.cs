using AutomationShell48.Core.Services;
using AutomationShell48.Core.Theming;
using AutomationShell48.Infrastructure.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutomationShell48.Tests
{
    [TestClass]
    public class ThemeServiceTests
    {
        [TestMethod]
        public void ThemeService_Updates_CurrentTheme()
        {
            var fakeManager = new FakeThemeResourceManager();
            var service = new ThemeService(fakeManager);

            service.ApplyTheme(ThemeKind.Dark);

            Assert.AreEqual(ThemeKind.Dark, service.CurrentTheme);
            Assert.AreEqual(ThemeKind.Dark, fakeManager.LastApplied);
        }

        private class FakeThemeResourceManager : IThemeResourceManager
        {
            public ThemeKind LastApplied { get; private set; }

            public void ApplyThemeResources(ThemeKind kind)
            {
                LastApplied = kind;
            }
        }
    }
}
