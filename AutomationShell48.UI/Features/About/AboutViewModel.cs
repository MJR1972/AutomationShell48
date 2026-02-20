using AutomationShell48.Core.MVVM;

namespace AutomationShell48.UI.Features.About
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";
            AboutText = "AutomationShell48 is a reusable .NET Framework 4.8 WPF shell template using pure MVVM and no external packages.";
        }

        public string AboutText { get; }
    }
}

