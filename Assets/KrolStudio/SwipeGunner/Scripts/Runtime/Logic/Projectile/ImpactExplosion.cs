using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace KrolStudio
{
    /// <summary>
    /// 子弹命中后的“爆炸”范围结算。
    ///
    /// 职责只有三件事：
    ///   1) 以命中点为球心，查询半径内的所有敌人（Physics.OverlapSphereNonAlloc）；
    ///   2) 按距离衰减对它们扣血（IHealth.TakeDamage）；
    ///   3) 在命中点播放一次爆炸特效。
    ///
    /// 注意：
    ///   - 这里【不会】创建/销毁任何刚体，也不会改 isKinematic / linearVelocity，
    ///     避免在 FixedUpdate 的物理步里改动 PhysX 场景（那是之前原生崩溃的高危区域）；
    ///   - 本方法没有递归调用自身，所以下面的静态缓冲区可以安全复用。
    /// </summary>
    public static class ImpactExplosion
    {
        /// <summary>范围查询的缓冲区大小。同屏敌人超过这个数量时，多出来的会被忽略。</summary>
        const int BufferSize = 32;

        /// <summary>
        /// 爆炸特效预制体（VFX_ExplosionSphere）制作时的参考半径（米）。
        /// 如果 SO_ProjectileConfig 里改了 explosionRadius，SpawnVfx 会按比例缩放特效，
        /// 保证"看到的球"和"实际伤害的球"一样大。
        /// </summary>
        const float VfxReferenceRadius = 2f;

        /// <summary>复用的碰撞体缓冲，避免每次命中都产生 GC（和 Projectile.overlapResults 一个思路）。</summary>
        static readonly Collider[] Buffer = new Collider[BufferSize];

        /// <summary>本次爆炸收集到的目标（先收集、后结算，避免结算过程中集合被改乱）。</summary>
        static readonly List<IHealth> Targets = new List<IHealth>(BufferSize);

        /// <summary>目标去重集合：同一个敌人可能被多个碰撞体命中（子物体/布娃娃骨头）。</summary>
        static readonly HashSet<IHealth> Seen = new HashSet<IHealth>();

        /// <summary>缓存 "Enemy" 层的 mask，避免每次命中都调用 LayerMask.GetMask（内部要做字符串查找）。</summary>
        static int explosionMask = -1;
        static int ExplosionMask
        {
            get
            {
                if (explosionMask < 0)
                    explosionMask = LayerMask.GetMask("Enemy");

                return explosionMask;
            }
        }

        /// <summary>
        /// 立即结算爆炸（最简单）。
        /// 注意：如果是在 Projectile.FixedUpdate 里调用，扣血/死亡会当场发生，
        /// 一次炸死很多敌人时会在同一个物理步里激活多套布娃娃。
        /// </summary>
        /// <param name="center">爆炸中心，通常是 SphereCast 的 hit.point。</param>
        /// <param name="fallbackDirection">兜底方向：目标正好在爆炸中心时，用它算击退方向。</param>
        /// <param name="config">子弹配置（提供半径、衰减、特效等参数）。</param>
        /// <param name="damage">爆炸基础伤害（调用方按当前攻击力算好传入）。</param>
        /// <param name="directTarget">被子弹直接命中的目标，默认跳过，避免双倍伤害。</param>
        /// <param name="sender">伤害来源，传子弹自身即可（击杀奖励会用 sender 做判断）。</param>
        public static void Resolve(Vector3 center, Vector3 fallbackDirection, ProjectileConfig config,
                                   int damage, IHealth directTarget, object sender)
        {
            SpawnVfx(center, config);
            Apply(center, fallbackDirection, config, damage, directTarget, sender);
        }

        /// <summary>
        /// 延迟一帧再结算伤害的版本：特效立刻播放，扣血/死亡挪到下一帧的 Update。
        ///
        /// 这样一次炸死一群敌人时，布娃娃激活不会全部挤在同一个物理步里。
        /// 如果压测时又遇到引擎原生崩溃，把 Projectile 里的 Resolve 换成这个即可。
        /// </summary>
        public static void ResolveDeferred(Vector3 center, Vector3 fallbackDirection, ProjectileConfig config,
                                           int damage, IHealth directTarget, object sender)
        {
            SpawnVfx(center, config);
            ApplyNextFrame(center, fallbackDirection, config, damage, directTarget, sender).Forget();
        }

        /// <summary>等待一帧后再执行真正结算。</summary>
        static async UniTaskVoid ApplyNextFrame(Vector3 center, Vector3 fallbackDirection,
                                                ProjectileConfig config, int damage,
                                                IHealth directTarget, object sender)
        {
            await UniTask.NextFrame();

            // 下一帧敌人可能已经死了/被回收了，Apply 内部会再做一次 IsAlive 过滤
            Apply(center, fallbackDirection, config, damage, directTarget, sender);
        }

        /// <summary>真正的范围结算逻辑。</summary>
        static void Apply(Vector3 center, Vector3 fallbackDirection, ProjectileConfig config,
                          int damage, IHealth directTarget, object sender)
        {
            // 半径 <= 0 视为关闭爆炸：直接返回，不做任何查询
            if (config.explosionRadius <= 0f)
                return;

            Targets.Clear();
            Seen.Clear();

            // 关键点：这里是“球形范围查询”，和 Projectile 里的 SphereCast（沿弹道的一条粗线）不是一回事。
            // Enemy 层的敌人根节点上挂着 BoxCollider + EnemyHealth，所以这个查询能一次拿到范围内的全部目标。
            int count = Physics.OverlapSphereNonAlloc(
                center, config.explosionRadius, Buffer, ExplosionMask, QueryTriggerInteraction.Collide);

            // ---------- 第一步：只收集目标 ----------
            for (int i = 0; i < count; i++)
            {
                Collider col = Buffer[i];
                if (col == null)
                    continue;

                // 用 GetComponentInParent：碰撞体可能挂在子物体/布娃娃骨头上，而血量组件在根节点上。
                // （Projectile 直击用的是 TryGetComponent，只因为敌人根节点自己带了 BoxCollider）
                IHealth health = col.GetComponentInParent<IHealth>();
                if (health == null || !health.IsAlive)
                    continue;

                // 直击目标一般已经被 Projectile.Attack 结算过了，默认跳过，避免双倍伤害。
                // 想让它也吃爆炸：在 SO_ProjectileConfig 里勾上 explosionAffectsDirectTarget。
                if (!config.explosionAffectsDirectTarget && object.ReferenceEquals(health, directTarget))
                    continue;

                // 去重：同一个敌人即使被多个碰撞体命中，也只结算一次
                if (!Seen.Add(health))
                    continue;

                Targets.Add(health);
            }

            // ---------- 第二步：统一结算伤害 ----------
            for (int i = 0; i < Targets.Count; i++)
            {
                IHealth health = Targets[i];

                // IHealth 的实现都是 MonoBehaviour，这里拿它的 Transform 算距离
                Component component = health as Component;
                Vector3 targetPos = component != null ? component.transform.position : center;

                // 距离衰减：爆炸中心 100%，到半径边缘变成 explosionFalloff（例如 0.3 = 30%）
                float distance = Vector3.Distance(center, targetPos);
                float t = Mathf.Clamp01(distance / config.explosionRadius);
                float factor = Mathf.Lerp(1f, config.explosionFalloff, t);

                // 至少 1 点伤害，避免衰减后变成 0 导致"炸了但没感觉"
                int finalDamage = Mathf.Max(1, Mathf.RoundToInt(damage * factor));

                // 击退方向：从爆炸中心指向目标（只取水平面，避免把敌人往天上顶）
                Vector3 push = targetPos - center;
                push.y = 0f;

                if (push.sqrMagnitude < 0.0001f)
                    push = fallbackDirection;   // 目标正好在中心时，用子弹飞行方向兜底

                push = push.normalized;

                // 走和直击完全一样的伤害入口：
                // Health.TakeDamage -> OnHealthChanged/OnDied -> EnemyDamageImpact（闪白、飘字、死亡表现）
                // 注意 sender 必须是子弹本身，击杀奖励里会用 sender 做判断
                health.TakeDamage(finalDamage, sender, new DamageDirectionEventArgs(finalDamage, push));

                // ==== [燃烧效果] 给范围内的每个目标挂上持续伤害（配置为 0 时内部自动跳过） ====
                BurnEffect.ApplyTo(health, config, sender);
                // ==== [燃烧效果] end ====
            }

            Targets.Clear();
            Seen.Clear();
        }

        /// <summary>在命中点生成爆炸特效。特效是纯表现，不参与物理。</summary>
        static void SpawnVfx(Vector3 center, ProjectileConfig config)
        {
            // 没配预制体就只做伤害，不报错、不影响流程
            if (config.explosionPrefab == null)
                return;

            GameObject vfx = Object.Instantiate(config.explosionPrefab, center, Quaternion.identity);

            // 让特效尺寸跟随配置里的爆炸半径（预制体按 VfxReferenceRadius 制作）
            if (VfxReferenceRadius > 0f && config.explosionRadius > 0f)
                vfx.transform.localScale = Vector3.one * (config.explosionRadius / VfxReferenceRadius);

            // 自动销毁，避免特效对象越积越多（0 表示不自动销毁，由预制体自己控制）
            if (config.explosionVfxLifetime > 0f)
                Object.Destroy(vfx, config.explosionVfxLifetime);
        }
    }
}