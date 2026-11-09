using Cysharp.Threading.Tasks;
using Zenject;

namespace KrolStudio
{
    public class GameLoopState : IState, IUpdatableState
    {
        readonly PlayerStateMachine carStateMachine;
        readonly IHudView hudView;
        readonly MovementTracker movementTracker;

        [Inject]
        public GameLoopState(PlayerStateMachine carStateMachine, 
                             IHudView hudView, 
                             MovementTracker movementTracker)
        {
            this.carStateMachine = carStateMachine;
            this.hudView = hudView;
            this.movementTracker = movementTracker;
        }

        public UniTask Enter()
        {
            carStateMachine.Enter<PlayerRunState>().Forget();
            return default;
        }

        public UniTask Exit()
        {
            return default;
        }

        public void Update()
        {
            hudView.UpdateMeterProgress(movementTracker);
        }
    }
}