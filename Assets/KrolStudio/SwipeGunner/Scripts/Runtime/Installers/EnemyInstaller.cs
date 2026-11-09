using Zenject;

namespace KrolStudio
{
    public class EnemyInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindEnemyStateMachine();
            BindStatesFactory();
            BindEnemyComponents();
        }

        void BindEnemyStateMachine()
        {
            Container.Bind<EnemyStateMachine>().AsSingle();

            Container.Bind<EnemyIdleState>().AsSingle();
            Container.Bind<EnemyInactiveState>().AsSingle();
            Container.Bind<EnemyHitState>().AsSingle();
            Container.Bind<EnemyChaseState>().AsSingle();
            Container.Bind<EnemyAttackState>().AsSingle();
            Container.Bind<EnemyWanderState>().AsSingle();
            Container.Bind<EnemyDeadState>().AsSingle();
        }

        void BindStatesFactory()
        {
            Container.BindInterfacesTo<StatesFactory>().AsSingle();
        }

        void BindEnemyComponents()
        {
            Container.Bind<IEnemyContext>().To<Enemy>().FromComponentOnRoot().AsSingle();
            Container.Bind<EnemyChaseTransitionChecker>().AsSingle();
        }
    }
}