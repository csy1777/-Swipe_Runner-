using System;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class Projectile : MonoBehaviour, IPoolReturnable<Projectile>
    {
        public event Action<Projectile> OnReturned;

        [SerializeField] private TrailRenderer trailRenderer;

        private ProjectileConfig config;
        private ILogService logService;
        private IPlayerContext playerContext;
        private Camera cam;

        private float flyTime;
        private Vector3 flyDirection;
        private Vector3 previousPosition;
        private bool returned;
        private int layerMask;
        private Collider[] overlapResults = new Collider[1]; // To avoid allocating every time

#if UNITY_EDITOR
        float debugDistance;
        Vector3 debugPreviousPosition;
#endif

        private void Start()
        {
            layerMask = LayerMask.GetMask("Enemy");
        }

        [Inject]
        public void Construct(ProjectileConfig config, 
                              IPlayerContext playerContext, 
                              ILogService logService,
                              [Inject(Id = "MainCamera")] Camera cam)
        {
            this.config = config;
            this.playerContext = playerContext;
            this.logService = logService;
            this.cam = cam;
        }

        public void ReturnToPool()
        {
            if (returned)
                return;

            returned = true;
            OnReturned?.Invoke(this);
        }

        public void Initialize(Transform firePoint)
        {
            if(firePoint == null)
            {
                Debug.LogError("FirePoint reference is not set. Please check the TurretPart component.");
                return;
            }

            flyDirection = firePoint.forward;
            transform.SetPositionAndRotation(firePoint.position, Quaternion.LookRotation(flyDirection));

            // Reset state
            returned = false;
            flyTime = 0f;
            previousPosition = transform.position;
            trailRenderer.Clear();
        }

        void FixedUpdate()
        {
            Move();
            CheckFlyTime();
            TryAttack();
        }

        void Move()
        {
            transform.position += flyDirection * config.flySpeed * Time.deltaTime;
        }

        void CheckFlyTime()
        {
            flyTime += Time.deltaTime;

            if (flyTime >= config.flyTime)
            {
                ReturnToPool();
            }
        }

        private void TryAttack()
        {
            Vector3 movement = transform.position - previousPosition;

            if (movement.sqrMagnitude > 0.0001f)
            {
                float distance = movement.magnitude;

#if UNITY_EDITOR
                debugDistance = distance;
                debugPreviousPosition = previousPosition;
#endif
                // ==== [爆炸效果] SphereCast 能给出精确命中点(hit.point)，传给 HandleAttack 作为爆炸中心 ====
                if (Physics.SphereCast(previousPosition, config.sphereCastRadius, flyDirection, out RaycastHit hit, distance, layerMask, QueryTriggerInteraction.Collide))
                {
                    HandleAttack(hit.collider, hit.point);
                }
                else
                {
                    // Fallback if the target is too close and SphereCast did not work
                    // 兜底情况下拿不到精确命中点，用子弹当前位置近似（误差最大约一帧的位移）
                    int hitsCount = Physics.OverlapSphereNonAlloc(transform.position, config.sphereCastRadius, overlapResults, layerMask, QueryTriggerInteraction.Collide);
                    if (hitsCount > 0)
                    {
                        HandleAttack(overlapResults[0], transform.position);
                    }
                }
                // ==== [爆炸效果] end ====
            }

            previousPosition = transform.position;
        }

        /// <summary>
        /// 处理命中：先对直接命中的目标造成普通伤害，再在命中点触发一次爆炸范围伤害。
        /// </summary>
        /// <param name="hit">被击中的碰撞体</param>
        /// <param name="point">命中点（爆炸中心）。SphereCast 时用 hit.point，兜底时用子弹位置。</param>
        private void HandleAttack(Collider hit, Vector3 point)
        {
            if (hit.TryGetComponent<IHealth>(out var healthHandler) && IsVisible(hit.transform.position))
            {
                // 1) 直击伤害：保持原逻辑不变
                Attack(healthHandler);

                // ==== [燃烧效果] 被直接命中的目标也会被点燃（范围内其他目标在 ImpactExplosion 里处理） ====
                //BurnEffect.ApplyTo(healthHandler, config, this);
                // ==== [燃烧效果] end ====

                // ==== [爆炸效果] 开始 ====
                // 2) 范围爆炸：对命中点半径内的其他敌人造成衰减伤害
                //    爆炸伤害 = 当前子弹伤害 × 配置倍率（至少 1 点）
                int splashDamage = Mathf.Max(1,
                    Mathf.RoundToInt(playerContext.PlayerStats.CurrentDamage * config.explosionDamageMultiplier));

                // 想完全关掉爆炸：注释掉下面这一行即可（其余代码不用动）
                ImpactExplosion.Resolve(point, flyDirection, config, splashDamage, healthHandler, this);

                // 如果压测时又出现引擎原生崩溃（一次炸死太多敌人），把上面这行换成：
                // ImpactExplosion.ResolveDeferred(point, flyDirection, config, splashDamage, healthHandler, this);
                // ==== [爆炸效果] end ====

                ReturnToPool();
            }
        }

        private bool IsVisible(Vector3 position) // Checks if the object is within the camera's view
        {
            Vector3 viewportPos = cam.WorldToViewportPoint(position);
            return viewportPos.z > 0f && viewportPos.x >= 0f && viewportPos.x <= 1f && viewportPos.y >= 0f && viewportPos.y <= 1f;
        }

        private void Attack(IHealth entity)
        {
            entity.TakeDamage(playerContext.PlayerStats.CurrentDamage, this, new DamageDirectionEventArgs(playerContext.PlayerStats.CurrentDamage, transform.forward));
        }


#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
                return;

            DrawCapsule(debugPreviousPosition, flyDirection, debugDistance, config.sphereCastRadius);

            // ==== [爆炸效果] 范围可视化：选中子弹时在 Scene 视图里画出爆炸半径，方便调参 ====
            if (config.explosionRadius > 0f)
            {
                Gizmos.color = new Color(1f, 0.45f, 0f, 0.35f);
                Gizmos.DrawSphere(transform.position, config.explosionRadius);
            }
            // ==== [爆炸效果] end ====
        }

        private void DrawCapsule(Vector3 start, Vector3 direction, float distance, float radius)
        {
            if (debugDistance < 0.001f)
                return;

            Vector3 end = start + direction * distance;

            int segments = 12;
            Gizmos.color = Color.red;

            // X and Y axes relative to the direction
            Vector3 axis1 = Vector3.Cross(direction, Vector3.up);
            if (axis1 == Vector3.zero)
                axis1 = Vector3.Cross(direction, Vector3.right);
            Vector3 axis2 = Vector3.Cross(direction, axis1);

            axis1.Normalize();
            axis2.Normalize();

            for (int i = 0; i < segments; i++)
            {
                float angle = (360f / segments) * i;
                float nextAngle = (360f / segments) * (i + 1);

                // Normalize and scale by radius
                Vector3 offset = Quaternion.AngleAxis(angle, direction) * axis1 * radius;
                Vector3 nextOffset = Quaternion.AngleAxis(nextAngle, direction) * axis1 * radius;

                // Rings at the ends
                Gizmos.DrawLine(start + offset, start + nextOffset);
                Gizmos.DrawLine(end + offset, end + nextOffset);

                // Side lines
                Gizmos.DrawLine(start + offset, end + offset);
            }
        }

        private void DrawCapsuleV2(Vector3 start, Vector3 direction, float distance, float radius)
        {
            Vector3 end = start + direction * distance;

            // Rotation to orient the capsule in the desired direction
            Quaternion rotation = Quaternion.LookRotation(direction);

            // Segments for smoothness
            int segments = 12;

            // Draw the body of the capsule (cylinder)
            for (int i = 0; i < segments; i++)
            {
                float angle = (360f / segments) * i;
                float nextAngle = (360f / segments) * (i + 1);

                Vector3 offset = rotation * new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * radius, Mathf.Sin(angle * Mathf.Deg2Rad) * radius, 0f);
                Vector3 nextOffset = rotation * new Vector3(Mathf.Cos(nextAngle * Mathf.Deg2Rad) * radius, Mathf.Sin(nextAngle * Mathf.Deg2Rad) * radius, 0f);

                Gizmos.color = Color.red;

                // Two rings at the ends
                Gizmos.DrawLine(start + offset, start + nextOffset);
                Gizmos.DrawLine(end + offset, end + nextOffset);

                // Lines between the rings
                Gizmos.DrawLine(start + offset, end + offset);
            }
        }
#endif

    }
}