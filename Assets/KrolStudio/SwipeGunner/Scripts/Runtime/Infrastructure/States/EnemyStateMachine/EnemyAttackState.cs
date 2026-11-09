using Cysharp.Threading.Tasks;
using UnityEngine;

namespace KrolStudio
{
    public class EnemyAttackState : IState, IUpdatableState
    {
        readonly IPlayerContext playerContext;
        readonly IEnemyContext enemyContext;
        readonly EnemyConfig config;

        public EnemyAttackState(EnemyConfig config,
                               IPlayerContext playerContext,
                               IEnemyContext enemyContext)
        {
            this.playerContext = playerContext;
            this.enemyContext = enemyContext;
            this.config = config;
        }

        public UniTask Enter()
        {
            enemyContext.AudioManager.PlaySound(GameConstants.Sounds.StickmanAttack);

            enemyContext.Animator.PlayBehindAttack(Attack);
            return default;
        }

        public UniTask Exit()
        {
            return default;
        }

        public void Update()
        {
            MoveTowardsPlayer();
        }

        void Attack()
        {
            playerContext.PlayerHealth.TakeDamage(enemyContext.EnemyStats.CurrentDamage, enemyContext, new DamageEventArgs(enemyContext.EnemyStats.CurrentDamage));

            Vector3 pushDirection = (playerContext.Transform.position - enemyContext.Transform.position).normalized;
            enemyContext.Health.TakeDamage(enemyContext.Health.Current, enemyContext, new DamageDirectionEventArgs(enemyContext.Health.Current, pushDirection));
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