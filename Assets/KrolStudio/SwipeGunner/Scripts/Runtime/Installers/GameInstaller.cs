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
        }
    }
}