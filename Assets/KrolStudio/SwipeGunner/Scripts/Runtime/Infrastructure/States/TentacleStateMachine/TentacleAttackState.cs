using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class TentacleAttackState : IState, IUpdatableState
    {
        readonly ITentacleContext tentacleContext;
        readonly TentacleConfig config;
        readonly TentacleTriggerChecker triggerChecker;
        readonly TentacleStateMachine stateMachine;
        readonly IPlayerContext playerContext;
        
        private float timer;
        private Collider[] results = new Collider[1];
        RaycastHit[] hits = new RaycastHit[1];
        private bool isAttack;
        private int targetCount;

        [Inject]
        public TentacleAttackState(TentacleConfig config,
                                   ITentacleContext tentacleContext,
                                   TentacleTriggerChecker triggerChecker,
                                   TentacleStateMachine stateMachine,
                                   IPlayerContext playerContext)
        {
            this.tentacleContext = tentacleContext;
            this.config = config;
            this.triggerChecker = triggerChecker;
            this.stateMachine = stateMachine;
            this.playerContext = playerContext;
        }

        public UniTask Enter()
        {
            return default;
        }

        public UniTask Exit()
        {
            return default;
        }

        public void Update()
        {

#if UNITY_EDITOR
            DrawCircle(tentacleContext.Transform.position, config.triggerDistance, tentacleContext.Transform.up, Color.green);
#endif

            LookAtPlayerY();

            if (!triggerChecker.CanTriggered())
            {
                stateMachine.Enter<TentacleIdleState>().Forget();
                return;
            }

            timer += Time.deltaTime;
            if (timer < config.attackInterval)
                return;

            timer = 0f;

            if(!isAttack)
            {
                FindTarget();
            }
        }

        public void FindTarget()
        {
            int count = Physics.OverlapSphereNonAlloc(
               tentacleContext.Transform.position,
               config.triggerDistance,
               results,
               config.detectableTargets,
               QueryTriggerInteraction.Collide
            );

            if (count > 0)
            {
                Vector3 origin = tentacleContext.Transform.position + tentacleContext.Transform.up;

                targetCount = Physics.SphereCastNonAlloc(
                       origin,
                       config.sphereCastRadius,
                       tentacleContext.Transform.forward,
                       hits,
                       config.sphereCastDistance,
                       //config.damageableTargets,
                       config.detectableTargets,
                       QueryTriggerInteraction.Collide
                   );

                if (targetCount > 0)
                {
                    isAttack = true;
                    tentacleContext.TentacleAnimator.PlayAttack(Attack);
                }
            }
        }

        public void Attack()
        {
            for (int i = 0; i < targetCount; i++)
            {
                IPlayerContext playerContext = hits[i].collider.gameObject.GetComponentInParent<IPlayerContext>();

                if (playerContext != null)
                {
                    playerContext.PlayerHealth.TakeDamage(tentacleContext.TentacleStats.CurrentDamage, this, new DamageEventArgs(tentacleContext.TentacleStats.CurrentDamage));
                }
                //else if (hits[i].collider.TryGetComponent(out IEnemyContext enemyContext))
                //{
                //    enemyContext.Health.TakeDamage(tentacleContext.TentacleStats.CurrentDamage, this, new DamageDirectionEventArgs(tentacleContext.TentacleStats.CurrentDamage, Vector3.up));
                //}
            }

            isAttack = false;
            timer = 0f;
        }

        void LookAtPlayerY()
        {
            Vector3 direction = playerContext.Transform.position - tentacleContext.Transform.position;
            direction.y = 0f; // Reset Y to ensure rotation is only horizontal

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                tentacleContext.Transform.rotation = Quaternion.Slerp(tentacleContext.Transform.rotation, targetRotation, Time.deltaTime * config.rotationSpeed);
            }
        }

        public void DebugSphereCast(Transform transform, float radius, float distance)
        {
            Vector3 origin = transform.position + transform.up;
            Vector3 direction = transform.forward;

            // Draw the central ray
            Debug.DrawRay(origin, direction * distance, Color.red);

            // Draw spheres at the start and end points (represented as circles)
            DrawCircle(origin, radius, transform.up, Color.green);
            DrawCircle(origin + direction * distance, radius, transform.up, Color.green);
        }

        private void DrawCircle(Vector3 center, float radius, Vector3 up, Color color, int segments = 24)
        {
            float angleStep = 360f / segments;
            Quaternion rotation = Quaternion.AngleAxis(angleStep, up);
            Vector3 forward = Vector3.forward * radius;

            Vector3 prevPoint = center + forward;

            for (int i = 0; i <= segments; i++)
            {
                forward = rotation * forward;
                Vector3 nextPoint = center + forward;
                Debug.DrawLine(prevPoint, nextPoint, color);
                prevPoint = nextPoint;
            }
        }
    }
}