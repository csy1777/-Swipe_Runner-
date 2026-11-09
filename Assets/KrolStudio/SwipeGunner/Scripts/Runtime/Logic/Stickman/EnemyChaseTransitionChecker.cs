
namespace KrolStudio
{
    public class EnemyChaseTransitionChecker
    {
        readonly IPlayerContext playerContext;
        readonly IEnemyContext enemyContext;
        readonly EnemyConfig config;

        public EnemyChaseTransitionChecker(IPlayerContext playerContext,
                                           IEnemyContext enemyContext, 
                                           EnemyConfig config)
        {
            this.playerContext = playerContext;
            this.enemyContext = enemyContext;
            this.config = config;
        }

        public bool CanTransitionToChase() 
        {
            return (enemyContext.Transform.position - playerContext.Transform.position).sqrMagnitude <= config.triggerDistance * config.triggerDistance;
        }
    }
}