using AutomationShell48.Core.MVVM;
using AutomationShell48.Core.Services;

namespace AutomationShell48.UI.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly ILogger _logger;

        public DashboardViewModel(ILogger logger)
        {
            _logger = logger;
            Title = "Main";
            SummaryText = "Welcome to the reusable AutomationShell48 dashboard.";
            _logger?.Info("Dashboard view loaded.");
        }

        public string SummaryText { get; }
    }
}
