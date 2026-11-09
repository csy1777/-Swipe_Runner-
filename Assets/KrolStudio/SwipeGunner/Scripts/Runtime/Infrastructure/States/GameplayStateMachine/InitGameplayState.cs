using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class InitGameplayState : IState
    {
        readonly GameplayStateMachine gameplayStateMachine;
        readonly PlayerStateMachine playerStateMachine;

        readonly ILogService logService;

        readonly ILevelPlacer levelPlacer;
        readonly EnemyPlacer enemyPlacer;
        readonly AudioManager audioManager;
        readonly IGameFactory gameFactory;
        readonly GameplayAssetsConfig assetsConfig;
        readonly ISaveLoadService saveLoadService;
        readonly LoadingCurtainProvider loadingCurtain;
        readonly AdsManager adsManager;

        private readonly IPersistentProgressService persistentProgress;

        [Inject]
        public InitGameplayState(ILogService logService, 
                                 GameplayStateMachine gameplayStateMachine,
                                 PlayerStateMachine playerStateMachine,
                                 EnemyPlacer enemyPlacer,
                                 ILevelPlacer levelPlacer,
                                 IGameFactory gameFactory,
                                 AudioManager audioManager,
                                 IPersistentProgressService persistentProgress,
                                 GameplayAssetsConfig assetsConfig,
                                 LoadingCurtainProvider loadingCurtain,
                                 ISaveLoadService saveLoadService,
                                 AdsManager adsManager)
        {
            this.gameplayStateMachine = gameplayStateMachine;
            this.playerStateMachine = playerStateMachine;

            this.logService = logService;

            this.levelPlacer = levelPlacer;
            this.enemyPlacer = enemyPlacer;

            this.gameFactory = gameFactory;
            this.audioManager = audioManager;

            this.saveLoadService = saveLoadService;
            this.persistentProgress = persistentProgress;

            this.assetsConfig = assetsConfig;
            this.loadingCurtain = loadingCurtain;

            this.adsManager = adsManager;
        }

        public async UniTask Enter()
        {
            Debug.Log("11111111111");
            logService.Log($"Entered {nameof(InitGameplayState)}");

            NotifyProgressReaderServices();
            InitGameSettings();

            levelPlacer.LoadLevel();

            enemyPlacer.Initialize(levelPlacer.CurrentLevel);

            audioManager.InitSounds();

            await UniTask.WhenAll(
                 playerStateMachine.Enter<PlayerIdleState>(),
                 gameplayStateMachine.Enter<LaunchState>()
            );

            await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: false);

            loadingCurtain.Instance.Hide();

            await UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false);

            adsManager.Initialize();
        }

        private void InitGameSettings()
        {
            assetsConfig.audioMixer.SetFloat("Music", Mathf.Log10(persistentProgress.Progress.MusicVolume) * 20);
            assetsConfig.audioMixer.SetFloat("Sfx", Mathf.Log10(persistentProgress.Progress.SfxVolume) * 20);
        }

        private void NotifyProgressReaderServices()
        {
            persistentProgress.Progress = saveLoadService.LoadProgress();

            foreach (var reader in saveLoadService.ProgressReader())
                reader.LoadProgress(persistentProgress.Progress);
        }

        public UniTask Exit()
        {
            return default;
        }
    }
}