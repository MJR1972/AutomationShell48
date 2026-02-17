using System;
using System.Collections.Concurrent;

namespace AutomationShell48.Infrastructure.Services
{
    public static class AppServices
    {
        private static readonly ConcurrentDictionary<Type, Lazy<object>> Singletons = new ConcurrentDictionary<Type, Lazy<object>>();
        private static readonly ConcurrentDictionary<Type, Func<object>> Factories = new ConcurrentDictionary<Type, Func<object>>();

        public static void RegisterSingleton<TInterface>(TInterface instance) where TInterface : class
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            var type = typeof(TInterface);
            Singletons[type] = new Lazy<object>(() => instance, true);
        }

        public static void Register<TInterface>(Func<TInterface> factory) where TInterface : class
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            Factories[typeof(TInterface)] = () => factory();
        }

        public static TInterface Resolve<TInterface>() where TInterface : class
        {
            var type = typeof(TInterface);

            if (Singletons.TryGetValue(type, out var singleton))
            {
                return (TInterface)singleton.Value;
            }

            if (Factories.TryGetValue(type, out var factory))
            {
                return (TInterface)factory();
            }

            throw new InvalidOperationException("Service is not registered: " + type.FullName);
        }
    }
}
