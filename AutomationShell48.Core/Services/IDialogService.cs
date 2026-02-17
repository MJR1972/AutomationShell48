using System;
using System.Threading.Tasks;
using AutomationShell48.Core.MVVM;

namespace AutomationShell48.Core.Services
{
    public interface IDialogService
    {
        event Action<DialogViewModelBase> DialogOpened;
        event Action DialogClosed;
        Task<bool?> ShowDialogAsync(DialogViewModelBase viewModel);
        void CloseDialog(bool? result = null);
    }
}
