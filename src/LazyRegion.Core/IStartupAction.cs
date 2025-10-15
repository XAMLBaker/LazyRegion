using Microsoft.Extensions.DependencyInjection;
using System;

namespace LazyRegion.Core
{
    public interface IStartupAction
    {
        void Execute();
    }

    public class StartupAction : IStartupAction
    {
        private readonly Action _action;
        public StartupAction(Action action) => _action = action;
        public void Execute() => _action ();
    }

    public class InitialNavigationStartupAction : IStartupAction
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Action<ILazyRegionManager> _configure;

        public InitialNavigationStartupAction(
            IServiceProvider serviceProvider,
            Action<ILazyRegionManager> configure)
        {
            _serviceProvider = serviceProvider;
            _configure = configure;
        }

        public void Execute()
        {
            var manager = _serviceProvider.GetRequiredService<ILazyRegionManager> ();
            _configure (manager);
        }
    }
}