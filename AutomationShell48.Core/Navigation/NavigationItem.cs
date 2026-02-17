using AutomationShell48.Core.MVVM;

namespace AutomationShell48.Core.Navigation
{
    public class NavigationItem : ObservableObject
    {
        private bool _isSelected;

        public NavigationItem(string key, string displayName, string iconKey)
        {
            Key = key;
            DisplayName = displayName;
            IconKey = iconKey;
        }

        public string Key { get; }

        public string DisplayName { get; }

        public string IconKey { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
