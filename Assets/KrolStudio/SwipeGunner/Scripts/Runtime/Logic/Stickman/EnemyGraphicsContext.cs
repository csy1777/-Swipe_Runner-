using UnityEngine;

namespace KrolStudio
{
    public interface IEnemyGraphicsContext
    {
        DamageFlashEffect DamageFlashEffect { get; }
        Rigidbody RagdollHolder { get; }
        EnemyAnimator EnemyAnimator { get; }
    }

    [RequireComponent(typeof(DamageFlashEffect))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(EnemyAnimator))]
    public class EnemyGraphicsContext : MonoBehaviour, IEnemyGraphicsContext
    {
        [SerializeField] Rigidbody ragdollHolder;
    
        DamageFlashEffect damageFlashEffect;
        EnemyAnimator enemyAnimator;

        public Rigidbody RagdollHolder => ragdollHolder;

        public DamageFlashEffect DamageFlashEffect
        {
            get
            {
                if (damageFlashEffect == null)
                    damageFlashEffect = GetComponent<DamageFlashEffect>();

                return damageFlashEffect;
            }
        }

        public EnemyAnimator EnemyAnimator
        {
            get
            {
                if(enemyAnimator == null)
                    enemyAnimator = GetComponent<EnemyAnimator>();

                return enemyAnimator;
            }
        }
    }
}