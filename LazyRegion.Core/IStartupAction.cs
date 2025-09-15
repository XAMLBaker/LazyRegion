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
}