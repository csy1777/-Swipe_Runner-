using Cysharp.Threading.Tasks;
using UnityEngine;

namespace KrolStudio
{
    public class EnemyIdleState : IState, IUpdatableState
    {
        readonly EnemyChaseTransitionChecker pursuitTransitionChecker;
        readonly EnemyStateMachine stateMachine;
        readonly EnemyConfig config;
        readonly IEnemyContext enemyContext;
        readonly IPlayerContext playerContext;
        readonly ILogService logService;

        float t;
        float duration;

        public EnemyIdleState(EnemyStateMachine stateMachine, 
                              EnemyConfig config,
                              IEnemyContext enemyContext,
                              IPlayerContext playerContext,
                              ILogService logService,
                              EnemyChaseTransitionChecker pursuitTransitionChecker)
        {
            this.stateMachine = stateMachine;
            this.pursuitTransitionChecker = pursuitTransitionChecker;
            this.enemyContext = enemyContext;
            this.playerContext = playerContext;
            this.logService = logService;
            this.config = config;
        }


        public UniTask Enter()
        {
            enemyContext.Animator.PlayMovement(false);

            t = 0;
            duration = Random.Range(config.idleDuration.x, config.idleDuration.y);

            return default;
        }

        public UniTask Exit()
        {
            return default;
        }

        bool IsPossibleToChase()
        {
            return playerContext.PlayerChaseTarget.IsPossibleToChase();
        }

        bool CanTransitionToWander()
        {
            t += Time.deltaTime;
            return t >= duration;
        }

        public void Update()
        {
            if (!IsPossibleToChase())
            {
                stateMachine.Enter<EnemyInactiveState>().Forget();
                return;
            }

            if(CanTransitionToWander())
            {
                stateMachine.Enter<EnemyWanderState>().Forget();
                return;
            }

            if(pursuitTransitionChecker.CanTransitionToChase())
            {
                stateMachine.Enter<EnemyChaseState>().Forget();
                return;
            }
        }
    }
}