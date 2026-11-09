using UnityEngine;

namespace KrolStudio
{
    public interface ITentacleGraphicsContext
    {
        TentacleAnimator TentacleAnimator { get; }
        DamageFlashEffect DamageFlashEffect { get; }
    }

    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(TentacleAnimator))]
    [RequireComponent(typeof(DamageFlashEffect))]
    public class TentacleGraphicsContext : MonoBehaviour, ITentacleGraphicsContext
    {
        TentacleAnimator tentacleAnimator;
        DamageFlashEffect damageFlashEffect;

        public TentacleAnimator TentacleAnimator
        {
            get
            {
                if(tentacleAnimator == null)
                {
                    tentacleAnimator = gameObject.GetComponent<TentacleAnimator>();
                }

                return tentacleAnimator;
            }
        }

        public DamageFlashEffect DamageFlashEffect
        {
            get
            {
                if (damageFlashEffect == null)
                {
                    damageFlashEffect = gameObject.GetComponent<DamageFlashEffect>();
                }

                return damageFlashEffect;
            }
        }
    }
}