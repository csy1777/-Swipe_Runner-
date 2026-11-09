namespace KrolStudio
{
    /// <summary>
    /// “燃烧”效果：给目标挂上持续伤害（DoT）+ 燃烧粒子。
    ///
    /// 作用对象由 <see cref="BulletEffects.ApplyShot"/> 决定：
    ///   - 没有范围技能：只有直击目标被点燃；
    ///   - 有范围技能：范围里所有人都被点燃。
    /// 所以这里不再判断"直击还是溅射"，给谁烧谁。
    ///
    /// 具体跳伤/粒子逻辑都在 <see cref="BurnEffect"/> 里，这里只负责接进流水线。
    /// </summary>
    public sealed class BurnBulletEffect : IBulletEffect
    {
        public void Apply(IHealth target, BulletEffectContext context)
        {
            // 数值来自本局快照（0 = 没解锁这颗效果）
            if (context.Stats.BurnDamagePerSecond <= 0f || context.Stats.BurnDuration <= 0f)
                return;

            BurnEffect.ApplyTo(target, context);
        }
    }
}