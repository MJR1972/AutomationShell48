using System.Collections.ObjectModel;
using AutomationShell48.Core.MVVM;

namespace AutomationShell48.Core.Navigation
{
    public class NavigationGroup : ObservableObject
    {
        private bool _isExpanded = true;

        public NavigationGroup(string title)
        {
            Title = title;
            Items = new ObservableCollection<NavigationItem>();
        }

        public string Title { get; }

        public ObservableCollection<NavigationItem> Items { get; }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }
    }
}
