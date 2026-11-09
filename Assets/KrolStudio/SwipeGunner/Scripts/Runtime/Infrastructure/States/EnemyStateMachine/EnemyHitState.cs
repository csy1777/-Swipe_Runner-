using Cysharp.Threading.Tasks;

namespace KrolStudio
{
    public class EnemyHitState : IState
    {
        readonly EnemyStateMachine stateMachine;
        readonly IEnemyContext enemyContext;

        readonly ILogService logService;

        public EnemyHitState(EnemyStateMachine stateMachine, 
                             IEnemyContext enemyContext,
                             ILogService logService)
        {
            this.stateMachine = stateMachine;
            this.enemyContext = enemyContext;
            this.logService = logService;
        }

        public UniTask Enter()
        {
            if(stateMachine.WasPreviousState(typeof(EnemyIdleState)))
            {
                enemyContext.Animator.PlayHit(() => {
                        if (enemyContext.Health.Current > 0)
                        {
                            stateMachine.Enter<EnemyIdleState>().Forget();
                        }
                    });
            }
            else if(stateMachine.WasPreviousState(typeof(EnemyWanderState)))
            {
                enemyContext.Animator.PlayHit(() => {
                        if (enemyContext.Health.Current > 0)
                        {
                            stateMachine.Enter<EnemyWanderState>().Forget();
                        }
                    });
            }
            else if (stateMachine.WasPreviousState(typeof(EnemyChaseState)))
            {
                enemyContext.Animator.PlayHit(() => {
                        if (enemyContext.Health.Current > 0)
                        {
                            stateMachine.Enter<EnemyChaseState>().Forget();
                        }
                    });
            }

            return default;
        }
        
        public UniTask Exit()
        {
            return default;
        }
    }
}