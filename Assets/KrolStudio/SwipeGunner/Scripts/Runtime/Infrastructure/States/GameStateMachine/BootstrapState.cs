using Cysharp.Threading.Tasks;
using Zenject;
using UnityEngine;

namespace KrolStudio
{
    public class BootstrapState : IState
    {
        readonly GameStateMachine gameStateMachine;
        readonly GameplayAssetsConfig gameplayAssetsConfig;
        readonly ILogService logService;
        readonly LoadingCurtainProvider curtainProvider;

        [Inject]
        public BootstrapState(GameStateMachine gameStateMachine, 
                              GameplayAssetsConfig gameplayAssetsConfig,
                              LoadingCurtainProvider curtainProvider,
                              ILogService logService)
        {
            this.gameStateMachine = gameStateMachine;
            this.logService = logService;
            this.curtainProvider = curtainProvider;
            this.gameplayAssetsConfig = gameplayAssetsConfig;
        }

        public UniTask Enter()
        {
            logService.Log($"Entered {nameof(BootstrapState)}");

            if(curtainProvider.Instance == null)
                curtainProvider.Instance = Object.Instantiate(gameplayAssetsConfig.curtainPrefab);

            gameStateMachine.Enter<LoadProgressState>().Forget();

            return default;
        }

        public UniTask Exit()
        {
            return default;
        }
    }
}