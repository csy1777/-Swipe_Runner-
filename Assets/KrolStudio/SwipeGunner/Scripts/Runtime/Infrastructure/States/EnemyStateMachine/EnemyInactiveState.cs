using Cysharp.Threading.Tasks;

namespace KrolStudio
{
    public class EnemyInactiveState : IState
    {
        readonly IEnemyContext enemyContext;
        readonly ILogService logService;

        public EnemyInactiveState(IEnemyContext enemyContext, ILogService logService)
        {
            this.enemyContext = enemyContext;
            this.logService = logService;
        }

        public UniTask Enter()
        {
            logService.Log($"Entered {nameof(EnemyInactiveState)}");

            enemyContext.Animator.PlayMovement(false);
            return default;
        }

        public UniTask Exit()
        {
            logService.Log($"Exit {nameof(EnemyInactiveState)}");
            return default;
        }
    }
}