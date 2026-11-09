using Cysharp.Threading.Tasks;
using System;
using Zenject;

namespace KrolStudio
{
    public class PlayerFinishState : IState
    {
        readonly GameplayStateMachine gameplayStateMachine;
        readonly IPlayerContext playerContext;

        [Inject]
        public PlayerFinishState(GameplayStateMachine gameplayStateMachine, 
                                IPlayerContext playerContext)
        {
            this.gameplayStateMachine = gameplayStateMachine;
            this.playerContext = playerContext;
        }

        public async UniTask Enter()
        {
            playerContext.PlayerFx.PlayConfetti();
            playerContext.PlayerChaseTarget.IsOnFinish = true;

            await UniTask.Delay(TimeSpan.FromSeconds(2d));
            gameplayStateMachine.Enter<WinGameState>().Forget();
        }

        public UniTask Exit()
        {
            playerContext.PlayerChaseTarget.IsOnFinish = false;

            return default;
        }
    }
}