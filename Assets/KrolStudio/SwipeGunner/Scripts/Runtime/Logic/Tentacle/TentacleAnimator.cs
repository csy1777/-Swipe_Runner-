using System;
using UnityEngine;

namespace KrolStudio
{
    public class TentacleAnimator : MonoBehaviour
    {
        readonly int Death = Animator.StringToHash(GameConstants.Animations.TentacleDeath);
        readonly int Attack = Animator.StringToHash(GameConstants.Animations.TentacleAttack);

        Animator animator;
        public Animator Animator
        {
            get
            {
                if (animator == null)
                {
                    animator = gameObject.GetComponent<Animator>();
                }

                return animator;
            }
        }

        Action OnAttackAnimationEnd;

        public void PlayDeath()
        {
            Animator.SetTrigger(Death);
        }

        public void PlayAttack(Action onEndHitAnimation = null)
        {
            if (this == null || Animator == null) return;
            this.OnAttackAnimationEnd = onEndHitAnimation;
            Animator.SetTrigger(Attack);
        }

        public void OnExitAttackState() // Animator`s event
        {
            if (this == null || Animator == null) return;
            OnAttackAnimationEnd.Invoke();
        }
    }
}