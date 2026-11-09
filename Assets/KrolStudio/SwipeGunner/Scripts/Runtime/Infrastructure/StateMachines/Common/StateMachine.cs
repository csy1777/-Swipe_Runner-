using Cysharp.Threading.Tasks;
using System;
using Zenject;

namespace KrolStudio
{
    public class StateMachine : IStateMachine
    {
        IStatesFactory statesFactory;
        ILogService logService;
        
        IExitableState prevState;
        IExitableState currentState;

        [Inject]
        void Construct(IStatesFactory statesFactory, ILogService logService)
        {
            this.statesFactory = statesFactory;
            this.logService = logService;
        }

        public void SetStatesFactory(IStatesFactory factory)
        {
            this.statesFactory = factory;
        }

        public void UpdateState()
        {
            if(currentState != null && currentState is IUpdatableState updateble)
            {
                updateble.Update();
            }
        }

        TState GetState<TState>() where TState : class, IExitableState
        {
            return statesFactory.GetState<TState>();
        }

        async UniTask<TState> ChangeState<TState>() where TState : class, IExitableState
        {
            TState newState = GetState<TState>();
            
            if(currentState != null)
                await currentState.Exit();

            prevState = currentState;
            currentState = newState;

            return newState;
        }

        public bool WasPreviousState(Type state)
        {
            return prevState.GetType() == state;
        }

        public bool IsCurrentState(Type state)
        {
            if (currentState == null) return false;
            return currentState.GetType() == state;
        }

        public async UniTask Enter<TState>() where TState : class, IState
        {
            IState state = await ChangeState<TState>();

            if(state != null)
            {
                await state.Enter();
            }
            else 
            {
                logService.LogError($"Failed to enter {typeof(TState)} state");
            }
        }

        public async UniTask Enter<TState, Param1>(Param1 param1) where TState : class, IState<Param1>
        {
            IState<Param1> state = await ChangeState<TState>();

            if (state != null)
            {
                await state.Enter(param1);
            }
            else
            {
                logService.LogError($"Failed to enter {typeof(TState)} state");
            }
        }
    }
}