using Zenject;

namespace KrolStudio
{
    public class PlayerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindPlayerStateMachine();
            BindStatesFactory();
            BindComponents();
            //BindInputService();
        }

        private void BindPlayerStateMachine()
        {
            Container.Bind<PlayerIdleState>().AsSingle();
            Container.Bind<PlayerRunState>().AsSingle();
            Container.Bind<PlayerFinishState>().AsSingle();
            Container.Bind<PlayerDeadState>().AsSingle();
        }

        //private void BindInputService()
        //{

        //    Container.BindInterfacesAndSelfTo<MobileInputService>().AsSingle();

            /* 
             // logic for different platforms
            #if UNITY_WEBGL
                        Container.BindInterfacesAndSelfTo<MobileInputService>().AsSingle();
            #elif UNITY_STANDALONE || UNITY_EDITOR
                Container.BindInterfacesAndSelfTo<MobileInputService>().AsSingle();
            #elif UNITY_IOS || UNITY_ANDROID
                Container.BindInterfacesAndSelfTo<MobileInputService>().AsSingle();
            #endif*/
        //}

        private void BindStatesFactory()
        {
            Container.BindInterfacesTo<StatesFactory>().AsSingle();
        }

        private void BindComponents()
        {
            // BindInterfacesAndSelfTo 同时注册接口和具体类
            // 这样既能用 IShooter 也能直接用 TurretShooting
            //Container.BindInterfacesAndSelfTo<TurretShooting>().AsSingle();
            Container.BindInterfacesAndSelfTo<TurretRotator>().AsSingle();
            Container.BindInterfacesAndSelfTo<TurretTrajectoryRenderer>().AsSingle();
        }
    }
}
