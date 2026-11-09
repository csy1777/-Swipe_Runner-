using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class PlayerDeadState : IState
    {
        readonly GameplayStateMachine gameplayStateMachine;
        readonly IPlayerContext playerContext;

        [Inject]
        public PlayerDeadState(GameplayStateMachine gameplayStateMachine,
                              ISaveLoadService saveLoadService,
                              IPlayerContext playerContext)
        {
            this.gameplayStateMachine = gameplayStateMachine;
            this.playerContext = playerContext;
        }

        public async UniTask Enter()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(3d));

            gameplayStateMachine.Enter<LoseGameState>().Forget();
        }

        public UniTask Exit()
        {
            Physics.gravity = new Vector3(0f, -9.81f, 0f);
            
            playerContext.PlayerFx.PlayNukeExplosion(false);

            return default;
        }
    }
}
