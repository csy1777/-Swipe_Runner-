using System;
using UnityEngine;

namespace KrolStudio
{
    public class EnemyAnimator : MonoBehaviour
    {
        readonly int Movement = Animator.StringToHash(GameConstants.Animations.EnemyMovement);
        readonly int MovementVal = Animator.StringToHash(GameConstants.Animations.EnemyMovementVal);
        readonly int Hit = Animator.StringToHash(GameConstants.Animations.EnemyHit);
        readonly int Attack = Animator.StringToHash(GameConstants.Animations.EnemyAttack);

        Action OnHitAnimationEnd;
        Action OnBehindAttackAnimationEnd;

        Animator animator;
        public Animator Animator
        {
            get
            {
                if (animator == null)
                    animator = GetComponent<Animator>();
                return animator;
            }
        }

        public void Enable(bool value)
        {
            Animator.enabled = value;
        }

        public void PlayMovement(bool value)
        {
            if (this == null || Animator == null) return;
            Animator.SetBool(Movement, value);
        }

        public void ChangingMovement(float targetValue, float accelerationMultiplier)
        {
            if (Mathf.Approximately(GetMovementVal(), targetValue)) return;

            SetMovementVal(Mathf.MoveTowards(GetMovementVal(),
                                          targetValue,
                                          accelerationMultiplier * Time.deltaTime));

            void SetMovementVal(float value)
            {
                Animator.SetFloat(MovementVal, value);
            }

            float GetMovementVal()
            {
                return Animator.GetFloat(MovementVal);
            }
        }

        public void PlayBehindAttack(Action onEndHitAnimation = null)
        {
            if (this == null || Animator == null) return;
            this.OnBehindAttackAnimationEnd = onEndHitAnimation;
            Animator.SetTrigger(Attack);
        }

        public void OnExitBehindAttackState() // Animator`s event
        {
            if (this == null || Animator == null) return;
            OnBehindAttackAnimationEnd.Invoke();
        }


        public void PlayHit(Action onEndHitAnimation = null)
        {
            if (this == null || Animator == null) return;
            this.OnHitAnimationEnd = onEndHitAnimation;
            Animator.SetTrigger(Hit);
        }

        public void OnExitHitState()
        {
            if (this == null || Animator == null) return;
            OnHitAnimationEnd.Invoke();
        }
    }
}