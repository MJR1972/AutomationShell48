namespace AutomationShell48.Core.MVVM
{
    public abstract class BaseViewModel : ObservableObject
    {
        private string _title;
        private bool _isBusy;
        private string _busyMessage;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string BusyMessage
        {
            get => _busyMessage;
            set => SetProperty(ref _busyMessage, value);
        }
    }
}
