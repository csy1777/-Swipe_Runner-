using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class GameplayInstaller : MonoInstaller
    {
        [SerializeField] Transform uiRoot;
        [SerializeField] HUDView hudView;

        [SerializeField] PlayerContext playerContextPrefab;
        [SerializeField] CameraSwitcher cameraSwitcher;
        [SerializeField] AudioManager globalAudioManager;
        [SerializeField] Camera mainCamera;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<TurretShooting>().AsSingle();
            Container.BindInterfacesAndSelfTo<MobileInputService>().AsSingle();
            BindGameplayStateMachine();
            BindStatesFactory();

            BindPlayerStateMachine();

            BindFactories();

            BindWindowService();
            BindPoolServices();

            BindPlacers();
            BindMainCamera();
            BindView();
            BindPlayer();
            BindSlideMoverContext();
            BindMovementTracker();

            BindCameraSwitcher();
            BindAudioManager();
            BindUpgradeSystem();
            BindPauseManager();
        }

       

        private void BindMainCamera()
        {
            Container.Bind<Camera>().WithId("MainCamera").FromInstance(mainCamera).AsSingle();
        }

        private void BindCameraSwitcher()
        {
            Container.Bind<CameraSwitcher>().FromInstance(cameraSwitcher).AsSingle();
        }

        private void BindView()
        {
            Container.Bind<Transform>().WithId("UIRoot").FromInstance(uiRoot).AsSingle();
            Container.BindInterfacesTo<HUDView>().FromInstance(hudView).AsSingle();
        }

        private void BindPauseManager()
        {
            Container.Bind<PauseManager>().AsSingle();
        }

        private void BindUpgradeSystem()
        {
            Container.BindInterfacesAndSelfTo<UpgradeSystem>().AsSingle();
        }

        private void BindAudioManager()
        {
            Container.Bind<AudioManager>().FromInstance(globalAudioManager).AsSingle();
        }

        private void BindGameplayStateMachine()
        {
            Container.Bind<GameplayStateMachine>().AsSingle();

            Container.Bind<GameplayBootstrapState>().AsSingle();
            Container.Bind<InitGameplayState>().AsSingle();
            Container.Bind<LaunchState>().AsSingle();
            Container.Bind<FireModeSelectState>().AsSingle();
            Container.Bind<TutorialState>().AsSingle();
            Container.Bind<GameLoopState>().AsSingle();
            Container.Bind<WinGameState>().AsSingle();
            Container.Bind<LoseGameState>().AsSingle();
        }

        private void BindStatesFactory()
        {
            Container.BindInterfacesTo<StatesFactory>().AsSingle();
        }

        private void BindMovementTracker()
        {
            Container.Bind<MovementTracker>().AsSingle();
        }

        private void BindSlideMoverContext()
        {
            Container.Bind<SlideMoverContext>().AsSingle();
        }

        private void BindPlayer()
        {
            Container.BindInterfacesAndSelfTo<PlayerContext>()
                     .FromComponentInNewPrefab(playerContextPrefab)
                     .AsSingle();
        }

        private void BindPlayerStateMachine()
        {
            // For PlayerStateMachine, the statesFactory from GameplayStateMachine will be assigned
            // statesFactory is overridden via SetStatesFactory(statesFactory)
            Container.Bind<PlayerStateMachine>().AsSingle();
        }

        private void BindWindowService()
        {
            Container.BindInterfacesTo<WindowService>().AsSingle();
        }


        private void BindPlacers()
        {
            Container.BindInterfacesAndSelfTo<LevelPlacer>().AsSingle();
            Container.BindInterfacesAndSelfTo<EnemyPlacer>().AsSingle();
        }

        private void BindFactories()
        {
            Container.BindInterfacesTo<GameFactory>().AsSingle();
            Container.BindInterfacesTo<WindowFactory>().AsSingle();
        }

        private void BindPoolServices()
        {
            Container.BindInterfacesTo<PoolObjectsContainer>().AsSingle();
            Container.BindInterfacesTo<InteractorPoolContainer>().AsSingle();
        }
    }
}