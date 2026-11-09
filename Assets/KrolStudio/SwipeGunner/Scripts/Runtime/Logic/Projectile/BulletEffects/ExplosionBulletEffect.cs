using UnityEngine;

namespace KrolStudio
{
    /// <summary>
    /// “爆炸”效果：额外伤害。
    ///
    /// 两种结算方式，由 <see cref="BulletEffects.ApplyShot"/> 决定调用到哪个目标：
    ///   - 直击目标（IsDirectHit = true）：
    ///       造成 Damage × ExplosionDirectExtra 的额外伤害。
    ///       默认倍率 1 = 再打一份，连同直击伤害合计 2 倍。
    ///       爆炸特效也只在这里播一次。
    ///   - 范围波及目标（IsDirectHit = false）：
    ///       造成 Damage × ExplosionDamageMultiplier × 距离衰减 的伤害（沿用旧行为）。
    ///
    /// 注意：没有范围技能（AoeRadius = 0）时，这个效果依然生效 —— 只是变成"直击双倍"。
    /// 这样"只拿爆炸、不拿范围"的 build 也不会变成废卡。
    /// </summary>
    public sealed class ExplosionBulletEffect : IBulletEffect
    {
        public void Apply(IHealth target, BulletEffectContext context)
        {
            BulletStats stats = context.Stats;

            if (context.IsDirectHit)
                ApplyDirect(target, context, stats);
            else
                ApplyAoe(target, context, stats);
        }

        /// <summary>直击：额外伤害 + 播一次爆炸特效。</summary>
        void ApplyDirect(IHealth target, BulletEffectContext context, BulletStats stats)
        {
            // 特效永远播（即使没配倍率），让玩家看到"这一枪是爆裂弹头"
            ExplosionVfx.Play(context.HitPoint, stats.AoeRadius, context.Config);

            if (stats.ExplosionDirectExtra <= 0f)
                return;

            int extraDamage = Mathf.Max(1,
                Mathf.RoundToInt(context.Damage * stats.ExplosionDirectExtra));

            // 走和普通伤害完全相同的入口：血条、飘字、死亡表现都会正常触发
            target.TakeDamage(extraDamage, context.Sender,
                new DamageDirectionEventArgs(extraDamage, context.HitDirection));
        }

        /// <summary>范围：对半径内的其他目标造成衰减伤害。</summary>
        void ApplyAoe(IHealth target, BulletEffectContext context, BulletStats stats)
        {
            // 没有范围 / 没配倍率：这个目标不吃爆炸伤害（但仍可能被燃烧/冰冻影响）
            if (stats.AoeRadius <= 0f || stats.ExplosionDamageMultiplier <= 0f)
                return;

            // IHealth 的实现都是 MonoBehaviour，这里拿它的 Transform 算距离
            Component component = target as Component;
            Vector3 targetPos = component != null ? component.transform.position : context.HitPoint;

            // 距离衰减：爆炸中心 100%，到半径边缘变成 ExplosionFalloff（例如 0.3 = 30%）
            float distance = Vector3.Distance(context.HitPoint, targetPos);
            float t = Mathf.Clamp01(distance / stats.AoeRadius);
            float factor = Mathf.Lerp(1f, stats.ExplosionFalloff, t);

            // 至少 1 点伤害，避免衰减后变成 0 导致"炸了但没感觉"
            int damage = Mathf.Max(1,
                Mathf.RoundToInt(context.Damage * stats.ExplosionDamageMultiplier * factor));

            // 击退方向：从爆炸中心指向目标（只取水平面，避免把敌人往天上顶）
            Vector3 push = targetPos - context.HitPoint;
            push.y = 0f;

            if (push.sqrMagnitude < 0.0001f)
                push = context.HitDirection;   // 目标正好在中心时，用子弹飞行方向兜底

            push = push.normalized;

            target.TakeDamage(damage, context.Sender,
                new DamageDirectionEventArgs(damage, push));
        }
    }
}