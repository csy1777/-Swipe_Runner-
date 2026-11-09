using System.Collections.Generic;
using UnityEngine;

namespace KrolStudio
{
    /// <summary>
    /// 子弹效果的统一入口。
    ///
    /// 一次命中分两步：
    ///   1) 决定"作用对象"：AoeRadius &lt;= 0 只有直击目标；&gt; 0 则半径内全部敌人；
    ///   2) 对集合里每个目标跑一遍效果列表（爆炸 / 燃烧 / 冰冻……）。
    ///
    /// 所以三个效果谁都不知道"范围"这件事，"范围"是独立的一层。
    /// 想加新效果：写一个 IBulletEffect 加进 Effects 数组即可，调用点不用动。
    ///
    /// 结构化的状态（冻结/燃烧）放在目标身上的组件里；这里的效果实例是无状态的。
    /// </summary>
    public static class BulletEffects
    {
        /// <summary>
        /// 已启用的效果，按数组顺序依次施加。
        /// 顺序影响"同一目标被多个效果处理"的先后（例如先算爆炸伤害还是先挂燃烧）。
        /// </summary>
        static readonly IBulletEffect[] Effects =
        {
            new ExplosionBulletEffect(),
            new BurnBulletEffect(),
            new FreezeBulletEffect(),
        };

        /// <summary>范围查询结果（静态复用，避免每发子弹都产生 GC）。</summary>
        static readonly List<IHealth> RadiusTargets = new List<IHealth>(32);

        /// <summary>范围查询去重集合（同一个敌人的多个碰撞体只算一次）。</summary>
        static readonly HashSet<IHealth> RadiusSeen = new HashSet<IHealth>();

        /// <summary>
        /// 结算一发子弹的全部效果。
        /// </summary>
        /// <param name="context">本次命中的上下文（IsDirectHit 应为 true）。</param>
        public static void ApplyShot(BulletEffectContext context)
        {
            // 配置缺失：什么都做不了
            if (context.Config == null || context.DirectTarget == null)
                return;

            // ---------- 1) 直击目标：永远走一遍 ----------
            // 注意这里【不】判断目标是否还活着：
            // 直击伤害可能已经把它打死了，但爆炸特效、范围伤害必须照常触发
            // （"一枪打死最后一只"也应该炸开），所以死亡由各个效果自己兜。
            ApplyAll(context.DirectTarget, context);

            // ---------- 2) 范围：只有解锁了范围技能才查 ----------
            float radius = context.Stats.AoeRadius;
            if (radius <= 0f)
                return;

            int count = EnemyQuery.FindInRadius(context.HitPoint, radius, RadiusTargets, RadiusSeen);

            for (int i = 0; i < count; i++)
            {
                IHealth target = RadiusTargets[i];

                // 直击目标上面已经结算过了，别重复
                if (ReferenceEquals(target, context.DirectTarget))
                    continue;

                ApplyAll(target, context.AsAoeTarget());
            }

            RadiusTargets.Clear();
            RadiusSeen.Clear();
        }

        /// <summary>
        /// 把"某一个目标"交给全部效果。
        ///
        /// 只过滤空引用，【不】过滤死亡目标：直击目标可能刚被直击伤害打死，
        /// 而爆炸特效仍然要播。所以每个效果实现都必须能安全处理已死亡的目标
        /// （TakeDamage 对 0 血目标是空操作；燃烧/冰冻自己会判 IsAlive）。
        /// </summary>
        public static void ApplyAll(IHealth target, BulletEffectContext context)
        {
            if (target == null)
                return;

            for (int i = 0; i < Effects.Length; i++)
            {
                IBulletEffect effect = Effects[i];

                if (effect == null)
                    continue;

                // 单个效果出错不能拖垮整条链路：
                // 这里是伤害结算循环，一个异常会中断后续目标/后续效果。
                try
                {
                    effect.Apply(target, context);
                }
                catch (System.Exception exception)
                {
                    Debug.LogException(exception, target as Component);
                }
            }
        }
    }
}