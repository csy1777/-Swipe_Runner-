namespace KrolStudio
{
    /// <summary>
    /// “冰冻”效果：把目标冻住一段时间，并把身体换成冰蓝材质。
    ///
    /// 作用对象由 <see cref="BulletEffects.ApplyShot"/> 决定：
    ///   - 没有范围技能：只冻直击目标；
    ///   - 有范围技能：范围内所有敌人统一冻结时长（不衰减）。
    ///
    /// 具体计时/解冻/换材质逻辑都在 <see cref="FreezeEffect"/> 里，这里只负责接进流水线。
    /// </summary>
    public sealed class FreezeBulletEffect : IBulletEffect
    {
        public void Apply(IHealth target, BulletEffectContext context)
        {
            // 数值来自本局快照（0 = 没解锁这颗效果）
            if (context.Stats.FreezeDuration <= 0f)
                return;

            FreezeEffect.ApplyTo(target, context);
        }
    }
}