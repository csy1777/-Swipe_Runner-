namespace KrolStudio
{
    /// <summary>
    /// 子弹命中后的附加效果（燃烧、冰冻、爆裂……）。
    ///
    /// 实现者只需要关心“给这个目标施加我自己的效果”，
    /// 目标是否存活、配置是否为空这类通用判断由 <see cref="BulletEffects"/> 统一做掉。
    ///
    /// 注意：实现里【不要】直接改物理状态（刚体/速度/isKinematic），
    /// 这个方法会在 FixedUpdate 的物理步里被调用，改动刚体会踩到之前的 PhysX 原生崩溃。
    /// </summary>
    public interface IBulletEffect
    {
        /// <summary>
        /// 对目标施加效果。
        /// </summary>
        /// <param name="target">命中目标（已保证非空且存活）。</param>
        /// <param name="context">本次命中的上下文（配置 / 伤害来源 / 直击或溅射）。</param>
        void Apply(IHealth target, BulletEffectContext context);
    }
}