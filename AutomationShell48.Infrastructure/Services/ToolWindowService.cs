using System;
using System.Collections.Concurrent;
using System.Windows;
using AutomationShell48.Core.MVVM;
using AutomationShell48.Core.Services;

namespace AutomationShell48.Infrastructure.Services
{
    public class ToolWindowService : IToolWindowService
    {
        private readonly Func<Window> _ownerAccessor;
        private readonly ConcurrentDictionary<Type, Func<object, Window>> _factories = new ConcurrentDictionary<Type, Func<object, Window>>();

        public ToolWindowService(Func<Window> ownerAccessor)
        {
            _ownerAccessor = ownerAccessor;
        }

        public void Register<TToolViewModel>(Func<TToolViewModel, object> windowFactory) where TToolViewModel : BaseViewModel
        {
            if (windowFactory == null) throw new ArgumentNullException(nameof(windowFactory));

            _factories[typeof(TToolViewModel)] = vm =>
            {
                var window = windowFactory((TToolViewModel)vm) as Window;
                if (window == null)
                {
                    throw new InvalidOperationException("Tool window factory must return a Window instance.");
                }

                return window;
            };
        }

        public void ShowTool<TToolViewModel>(TToolViewModel viewModel) where TToolViewModel : BaseViewModel
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
            if (!_factories.TryGetValue(typeof(TToolViewModel), out var factory))
            {
                throw new InvalidOperationException("No tool window registration for " + typeof(TToolViewModel).FullName);
            }

            var window = factory(viewModel);
            var owner = _ownerAccessor?.Invoke();
            if (owner != null && owner != window)
            {
                window.Owner = owner;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            window.Show();
            window.Activate();
        }
    }
}
