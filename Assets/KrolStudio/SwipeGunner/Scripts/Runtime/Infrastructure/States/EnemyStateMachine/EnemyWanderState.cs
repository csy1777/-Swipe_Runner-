using Cysharp.Threading.Tasks;
using UnityEngine;

namespace KrolStudio
{
    public class EnemyWanderState : IState, IUpdatableState
    {
        readonly EnemyStateMachine stateMachine;
        readonly EnemyConfig config;
        readonly IEnemyContext enemyContext;
        readonly IPlayerContext playerContext;
        readonly EnemyChaseTransitionChecker pursuitTransitionChecker;
        readonly ILogService logService;

        Vector3 destenationPoint;

        public EnemyWanderState(EnemyStateMachine stateMachine, 
                                EnemyConfig config,
                                ILogService logService,
                                IEnemyContext enemyContext,
                                EnemyChaseTransitionChecker pursuitTransitionChecker,
                                IPlayerContext playerContext)
        {
            this.stateMachine = stateMachine;
            this.enemyContext = enemyContext;
            this.pursuitTransitionChecker = pursuitTransitionChecker;
            this.playerContext = playerContext;
            this.logService = logService;
            this.config = config; 
        }

        public UniTask Enter()
        {
            enemyContext.Animator.PlayMovement(true);
            destenationPoint = GetRandomPointInCircle(enemyContext.SpawnPosition, config.wanderZoneRadius);

            return default;
        }

        public UniTask Exit()
        {
            enemyContext.Animator.PlayMovement(false);
            return default;
        }

        public void Update()
        {
            if(!IsPossibleToChase())
            {
                stateMachine.Enter<EnemyInactiveState>().Forget();
                return;
            }

            enemyContext.Animator.ChangingMovement(config.walkingThreshold, config.accelerationMultiplier);

            if (pursuitTransitionChecker.CanTransitionToChase())
            {
                stateMachine.Enter<EnemyChaseState>().Forget();
                return;
            }

            MoveTowardsPoint();

            if (CanTransitionToIdle())
            {
                stateMachine.Enter<EnemyIdleState>().Forget();
                return;
            }
        }

        bool CanTransitionToIdle()
        {
            return Vector3.Distance(enemyContext.Transform.position, destenationPoint) < config.wanderArrivalDistance;
        }

        bool IsPossibleToChase()
        {
            return playerContext.PlayerChaseTarget.IsPossibleToChase();
        }

        Vector3 GetRandomPointInCircle(Vector3 center, float radius)
        {
            Vector2 randomPoint2D = Random.insideUnitCircle * radius;
            return center + new Vector3(randomPoint2D.x, 0f, randomPoint2D.y);
        }

        void MoveTowardsPoint()
        {
            Vector3 currentPosition = enemyContext.Transform.position;
            Vector3 targetPosition = destenationPoint;

            enemyContext.Transform.position = Vector3.MoveTowards(
                currentPosition,
                targetPosition,
                config.walkingSpeed * Time.deltaTime
            );

            enemyContext.Transform.LookAt(targetPosition);
        }
    }
}