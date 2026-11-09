using Cysharp.Threading.Tasks;
using Zenject;

namespace KrolStudio
{
    public class TutorialScreen : WindowBase
    {
        GameplayStateMachine gameplayStateMachine;

        [Inject]
        void Construct(GameplayStateMachine gameplayStateMachine)
        {
            this.gameplayStateMachine = gameplayStateMachine;
        }

        protected override void CloseWindow()
        {
            base.CloseWindow();
            gameplayStateMachine.Enter<GameLoopState>().Forget();
        }

        protected override void Initialize()
        {
        }

        protected override void SubscribeUpdates()
        {
        }

        protected override void Cleanup()
        {
        }
    }
}