
namespace KrolStudio
{
    public interface IStatesFactory
    {
        TState GetState<TState>() where TState : class, IExitableState;
    }
}