using Zenject;

namespace KrolStudio
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindGameStateMachine();
            BindStatesFactory();
            BindServices();
            BindVibration();
            BindAdsManager();
        }

        private void BindAdsManager()
        {
            Container.BindInterfacesAndSelfTo<AdsManager>().AsSingle();
        }

        void BindVibration()
        {
            Container.Bind<Vibration>().AsSingle();
        }

        void BindGameStateMachine()
        {
            Container.Bind<GameStateMachine>().AsSingle();
            Container.Bind<BootstrapState>().AsSingle();
            Container.Bind<LoadProgressState>().AsSingle();
        }

        void BindStatesFactory()
        {
            Container.BindInterfacesTo<StatesFactory>().AsSingle();
        }

        void BindServices()
        {
            Container.BindInterfacesAndSelfTo<LoadingCurtainProvider>().AsSingle();

            Container.BindInterfacesTo<SceneLoader>().AsSingle();
            Container.BindInterfacesTo<LogService>().AsSingle();

            Container.BindInterfacesTo<PersistentProgressService>().AsSingle();
            Container.BindInterfacesTo<SaveLoadService>().AsSingle();

            Container.BindInterfacesTo<WalletService>().AsSingle();

            // 本局子弹数值（局内 buff）。
            // 必须放 ProjectContext：每关都会重载场景，局内 buff 要跨关保留；
            // 失败时由 LoseGameState 调 Reset 清空。
            Container.BindInterfacesAndSelfTo<RunBuffService>().AsSingle();
        }
    }
}