using Zenject;

namespace KrolStudio
{
    public class StatesFactory : IStatesFactory
    {
        readonly DiContainer diContainer;

        [Inject]
        public StatesFactory(DiContainer diContainer)
        {
            this.diContainer = diContainer;
        }

        public TState GetState<TState>() where TState : class, IExitableState
        {
            return diContainer.Resolve<TState>();
        }
    }
}

