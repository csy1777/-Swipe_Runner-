using UnityEngine;

namespace KrolStudio
{
    /// <summary>
    /// 冰冻（冻结）：把敌人原地冻住一段时间，并把外观换成冰蓝色材质。
    ///
    /// 工作机制（和 BurnEffect 对称，但不会自我销毁）：
    ///   1) 子弹命中时调用 <see cref="ApplyTo"/>，第一次命中才 AddComponent，之后常驻复用；
    ///   2) 冻结 = 调用 <see cref="Enemy.SetFrozen"/>(true)：停状态机推进 + 动画速度归零；
    ///      同时把敌人身体的材质换成 <see cref="ProjectileConfig.freezeMaterial"/>（冰蓝）；
    ///   3) 每帧倒数 remaining，重复命中只刷新时长、不叠层；
    ///   4) 时长结束 / 目标死亡 / 被池回收时解冻、恢复原材质；
    ///   5) 解冻后敌人从冻住那一刻的状态继续跑（不重置行为、不回 Idle）。
    ///
    /// 外观为什么交给 DamageFlashEffect 而不是自己换材质：
    ///   那个组件本来就持有渲染器列表和原始材质。如果这里直接换材质，
    ///   命中闪白结束时它会恢复成原始材质、把冰蓝抹掉；反过来这里结束时又会盖掉闪白。
    ///   统一走 DamageFlashEffect.SetPersistentMaterial 就不会互相打架。
    ///
    /// 为什么用"换材质"而不是"给材质乘颜色"：
    ///   敌人身体是 M_BlackStickman = 白色底 + 黑色贴图，单纯乘色改不动黑色部分
    ///   （黑 × 蓝 还是黑），所以只能整体替换成一块冰蓝材质。
    ///
    /// 为什么组件常驻而不是像 BurnEffect 那样用完 Destroy：
    ///   Destroy 是延迟到帧末执行的，如果在同一帧里敌人回池又被冻住，
    ///   GetComponent 会拿到"等待销毁"的旧组件，Setup 复活它之后它仍然会被销毁，
    ///   结果是敌人保持冻结但再也没人解冻。常驻组件完全没有这个问题，也省掉每次 AddComponent。
    ///
    /// 注意：这里【不】改刚体 / isKinematic / velocity，避免踩到 PhysX 原生崩溃。
    /// </summary>
    public class FreezeEffect : MonoBehaviour
    {
        /// <summary>被冻住的目标（血量组件在敌人根节点上）。</summary>
        IHealth health;

        /// <summary>敌人本体。用具体类型而不是 IEnemyContext，是为了能用 Unity 的 null 判断兜住"已销毁"。</summary>
        Enemy enemy;

        /// <summary>冻结时长（施加时的快照，来自 BulletStats）。</summary>
        float duration;

        /// <summary>冰蓝材质（来自 ProjectileConfig 的资产）。</summary>
        Material freezeMaterial;

        /// <summary>距解冻还剩多少秒。</summary>
        float remaining;

        /// <summary>当前是否处于冻结中。</summary>
        bool frozen;

        /// <summary>是否正在冻结（调试 / 外部查询用）。</summary>
        public bool IsActive => frozen;

        /// <summary>
        /// 施加冰冻。重复命中会刷新时长，不会叠层、不会重复换材质。
        /// </summary>
        /// <param name="target">目标血量组件</param>
        /// <param name="context">本次命中的上下文（时长读 Stats，材质读 Config）</param>
        public static void ApplyTo(IHealth target, in BulletEffectContext context)
        {
            // 目标已死 / 关闭了冰冻：直接忽略
            if (target == null || !target.IsAlive)
                return;
            if (context.Stats.FreezeDuration <= 0f)
                return;

            // IHealth 的实现都是 MonoBehaviour，挂在敌人根节点上
            Component component = target as Component;
            if (component == null)
                return;

            // 冰冻是靠"停敌人的状态机"实现的，所以只支持 Stickman（Enemy）。
            // 触手（Tentacle）走另一套 TentacleStateMachine，暂时不吃这个效果；
            // 拿不到 Enemy 就直接返回，避免挂一个什么都不做的组件。
            Enemy owner = component.GetComponent<Enemy>();
            if (owner == null)
                return;

            // 第一次命中才挂组件，之后一直复用（见类注释里关于 Destroy 竞态的说明）
            FreezeEffect freeze = component.GetComponent<FreezeEffect>();
            if (freeze == null)
                freeze = component.gameObject.AddComponent<FreezeEffect>();

            freeze.Setup(owner, target, context);
        }

        void Setup(Enemy owner, IHealth target, in BulletEffectContext context)
        {
            health = target;
            enemy = owner;

            // 把数值/材质拷到自己身上：之后本局数值再变化，不影响已经冻住的目标
            duration = context.Stats.FreezeDuration;
            freezeMaterial = context.Config != null ? context.Config.freezeMaterial : null;

            // 重复命中：刷新时长，不叠加
            remaining = Mathf.Max(remaining, duration);

            frozen = true;

            // SetFrozen 是幂等的：已经冻结时重复调用没有副作用。
            // 这里每次都调，是为了兜住"敌人回池时 Enemy.isFrozen 被 OnEnable 复位"的边界情况。
            enemy.SetFrozen(true);

            // 换冰蓝材质（配置留空则不改变外观）
            ApplyFreezeMaterial();
        }

        void Update()
        {
            // 没在冻结中：什么都不做（组件常驻，但每帧只多一次 bool 判断）
            if (!frozen)
                return;

            // 敌人被销毁 / 目标死亡：立即解冻
            if (enemy == null || health == null || !health.IsAlive)
            {
                End();
                return;
            }

            remaining -= Time.deltaTime;

            if (remaining <= 0f)
                End();
        }

        /// <summary>结束冻结：恢复敌人行为 + 恢复原材质。可以安全地重复调用。</summary>
        void End()
        {
            if (!frozen)
                return;

            frozen = false;
            remaining = 0f;

            // enemy 可能已经被销毁（例如整关重载），用 Unity 的 null 判断兜一下
            if (enemy != null)
            {
                enemy.SetFrozen(false);
                RestoreMaterial();
            }
        }

        /// <summary>把敌人身体换成冰蓝材质（经由 DamageFlashEffect，避免和闪白打架）。</summary>
        void ApplyFreezeMaterial()
        {
            if (freezeMaterial == null)
                return;

            if (enemy == null || enemy.DamageFlashEffect == null)
                return;

            enemy.DamageFlashEffect.SetPersistentMaterial(freezeMaterial);
        }

        /// <summary>恢复敌人原本的材质。</summary>
        void RestoreMaterial()
        {
            if (enemy == null || enemy.DamageFlashEffect == null)
                return;

            enemy.DamageFlashEffect.ClearPersistentMaterial();
        }

        /// <summary>
        /// 敌人被池子回收 / 被关闭时解冻。
        /// 这一步是必须的：否则敌人复用后还会带着上一局的冻结状态和冰蓝材质。
        /// </summary>
        void OnDisable()
        {
            End();
        }
    }
}