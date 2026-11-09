using Zenject;

namespace KrolStudio
{
    public class TentacleInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindTentacleStateMachine();
            BindStatesFactory();
            BindTentacleComponents();
        }

        void BindTentacleStateMachine()
        {
            Container.Bind<TentacleStateMachine>().AsSingle();

            Container.Bind<TentacleIdleState>().AsSingle();
            Container.Bind<TentacleAttackState>().AsSingle();
            Container.Bind<TentacleDeadState>().AsSingle();
        }

        void BindStatesFactory()
        {
            Container.BindInterfacesTo<StatesFactory>().AsSingle();
        }

        void BindTentacleComponents()
        {
            Container.Bind<ITentacleContext>().To<Tentacle>().FromComponentOnRoot().AsSingle();
            Container.Bind<TentacleTriggerChecker>().AsSingle();
        }
    }
}