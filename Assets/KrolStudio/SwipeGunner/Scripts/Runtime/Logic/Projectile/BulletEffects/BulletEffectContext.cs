using UnityEngine;

namespace KrolStudio
{
    /// <summary>
    /// 施加子弹效果时向下传递的上下文。
    ///
    /// 只读结构体：每次命中只产生一个（范围模式下每个目标一个），不产生堆分配。
    ///
    /// 数值和资产是分开的：
    ///   - Stats  → 会随卡牌变化的数值（范围半径、爆炸倍率、燃烧/冰冻参数）；
    ///   - Config → 不会变的资产（爆炸粒子、燃烧粒子、冰冻材质）。
    ///
    /// 这样效果既不需要依赖注入，也不会去写全局 ScriptableObject。
    /// </summary>
    public readonly struct BulletEffectContext
    {
        /// <summary>子弹配置：只放资产（粒子预制体、材质）。数值请读 Stats。</summary>
        public readonly ProjectileConfig Config;

        /// <summary>本局的数值快照（见 BulletStats / RunBuffService）。</summary>
        public readonly BulletStats Stats;

        /// <summary>伤害来源，一般就是子弹自身（击杀奖励会用 sender 做判断）。</summary>
        public readonly object Sender;

        /// <summary>
        /// 命中点（世界坐标）。
        /// 范围查询以它为圆心，爆炸特效也放在这里。
        /// </summary>
        public readonly Vector3 HitPoint;

        /// <summary>子弹飞行方向，给击退方向 / 特效朝向兜底。</summary>
        public readonly Vector3 HitDirection;

        /// <summary>这颗子弹"直接命中"的目标（整个 flow 里固定不变）。</summary>
        public readonly IHealth DirectTarget;

        /// <summary>本次直击造成的伤害。效果按它换算自己的伤害（爆炸 = 该值 × 倍率）。</summary>
        public readonly int Damage;

        /// <summary>
        /// 当前正在结算的这个目标，是不是"直击目标"。
        /// 范围模式下每个目标都会跑一遍效果，爆炸靠它区分"双倍"和"衰减伤害"。
        /// </summary>
        public readonly bool IsDirectHit;

        public BulletEffectContext(ProjectileConfig config,
                                   BulletStats stats,
                                   object sender,
                                   Vector3 hitPoint,
                                   Vector3 hitDirection,
                                   IHealth directTarget,
                                   int damage,
                                   bool isDirectHit)
        {
            Config = config;
            Stats = stats;
            Sender = sender;
            HitPoint = hitPoint;
            HitDirection = hitDirection;
            DirectTarget = directTarget;
            Damage = damage;
            IsDirectHit = isDirectHit;
        }

        /// <summary>
        /// 生成一份"当前目标是范围波及目标"的上下文。
        /// 只把 IsDirectHit 改成 false，其余（命中点、直击目标、伤害）保持不变。
        /// </summary>
        public BulletEffectContext AsAoeTarget()
        {
            return new BulletEffectContext(
                Config, Stats, Sender, HitPoint, HitDirection, DirectTarget, Damage, false);
        }
    }
}