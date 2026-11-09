using Cysharp.Threading.Tasks;
using UnityEngine;

namespace KrolStudio
{
    public class EnemyChaseState : IState, IUpdatableState
    {
        readonly EnemyStateMachine stateMachine;
        readonly IPlayerContext playerContext;
        readonly IEnemyContext enemyContext;
        readonly ILogService logService;
        
        EnemyConfig config;

        public EnemyChaseState(EnemyStateMachine stateMachine, 
                               EnemyConfig config,
                               IPlayerContext playerContext, 
                               IEnemyContext enemyContext,
                               ILogService logService)
        {
            this.stateMachine = stateMachine;
            this.playerContext = playerContext;
            this.enemyContext = enemyContext;
            this.config = config;
            this.logService = logService;
        }

        public UniTask Enter()
        {
            enemyContext.AudioManager.PlaySound(GameConstants.Sounds.GetRandomStickmanReaction());
            enemyContext.Animator.PlayMovement(true);
            return default;
        }

        public UniTask Exit()
        {
            enemyContext.Animator.PlayMovement(false);
            return default;
        }

        public void Update()
        {
            enemyContext.Animator.ChangingMovement(config.runningThreshold, config.accelerationMultiplier);

            MoveTowardsPlayer();

            if (TryAttack()) return;
            if (TryBehindAttack()) return;

            if (!IsPossibleToChase() || IsTargetLost())
                stateMachine.Enter<EnemyInactiveState>().Forget();
        }

        bool IsTargetLost()
        {
            return IsBehindTarget() &&
                   DistanceToTargetSquared() > config.chaseStopDistance * config.chaseStopDistance;
        }

        float DistanceToTargetSquared()
        {
            return (enemyContext.Transform.position - playerContext.Transform.position).sqrMagnitude;
        }

        public bool IsBehindTarget()
        {
            Vector3 toTarget = (playerContext.Transform.position - enemyContext.Transform.position).normalized;
            float dot = Vector3.Dot(playerContext.Transform.forward, toTarget);
            //logService.Log("IsBehindTarget: " + dot);
            return dot > config.backAngleThreshold; // backAngleThreshold Provides a small "tolerance angle"
        }

        bool IsPossibleToChase()
        {
            return playerContext.PlayerChaseTarget.IsPossibleToChase();
        }

        bool TryBehindAttack()
        {
            if(IsBehindTarget() && DistanceToTargetSquared() < config.behindAttackDistance * config.behindAttackDistance)
            {
                stateMachine.Enter<EnemyAttackState>().Forget();
                return true;
            }

            return false;
        }

        bool TryAttack()
        {
            if (RemainingDistanceToPoint() < config.attackDistance * config.attackDistance)
            {
                Attack();
                return true;
            }

            return false;
        }

        void Attack()
        {
            playerContext.PlayerHealth.TakeDamage(enemyContext.EnemyStats.CurrentDamage, enemyContext, new DamageEventArgs(enemyContext.EnemyStats.CurrentDamage));

            Vector3 pushDirection = (enemyContext.Transform.position - playerContext.Transform.position).normalized;
            enemyContext.Health.TakeDamage(enemyContext.Health.Current, enemyContext, new DamageDirectionEventArgs(enemyContext.Health.Current, pushDirection)); 
        }

        float RemainingDistanceToPoint()
        {
            Vector3 targetPosition = playerContext.PlayerChaseTarget.GetClosestTargetPoint(enemyContext.Transform.position);
            return (enemyContext.Transform.position - targetPosition).sqrMagnitude;
        }

        void MoveTowardsPlayer2()
        {
            Vector3 currentPosition = enemyContext.Transform.position;
            Vector3 targetPosition = playerContext.Transform.position;

            enemyContext.Transform.position = Vector3.MoveTowards(
                currentPosition,
                targetPosition,
                config.speed * Time.deltaTime
            );

            enemyContext.Transform.LookAt(targetPosition);
        }
        
        void MoveTowardsPlayer()
        {
            Vector3 directionToTarget = (playerContext.Transform.position - enemyContext.Transform.position).normalized;
            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

                enemyContext.Transform.rotation = Quaternion.Slerp(
                    enemyContext.Transform.rotation,
                    targetRotation,
                    config.rotationSpeed * Time.deltaTime
                );
            }

            enemyContext.Transform.position += enemyContext.Transform.forward * config.speed * Time.deltaTime;
        }
    }
}