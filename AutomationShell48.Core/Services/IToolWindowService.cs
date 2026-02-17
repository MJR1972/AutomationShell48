using System;
using AutomationShell48.Core.MVVM;

namespace AutomationShell48.Core.Services
{
    public interface IToolWindowService
    {
        void Register<TToolViewModel>(Func<TToolViewModel, object> windowFactory) where TToolViewModel : BaseViewModel;
        void ShowTool<TToolViewModel>(TToolViewModel viewModel) where TToolViewModel : BaseViewModel;
    }
}
