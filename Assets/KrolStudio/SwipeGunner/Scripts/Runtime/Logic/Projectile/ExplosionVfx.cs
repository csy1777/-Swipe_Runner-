using UnityEngine;

namespace KrolStudio
{
    /// <summary>
    /// 爆炸特效。纯表现：不结算伤害、不碰物理。
    ///
    /// 以前的 ImpactExplosion 把"查范围 + 扣血 + 播特效"写在一起，
    /// 导致燃烧只能寄生在爆炸的溅射循环里。现在伤害结算搬进了 ExplosionBulletEffect，
    /// 这里只剩下"在命中点播一次爆炸"。
    /// </summary>
    public static class ExplosionVfx
    {
        /// <summary>
        /// 爆炸特效预制体（VFX_ExplosionSphere）制作时的参考半径（米）。
        /// 实际半径和它不同时按比例缩放，保证"看到的球"和"实际伤害的球"一样大。
        /// </summary>
        const float VfxReferenceRadius = 2f;

        /// <summary>
        /// 没有范围技能时（AoeRadius = 0）的最小视觉半径。
        /// 否则缩放会变成 0，特效直接看不见。
        /// </summary>
        const float MinVisualRadius = 1f;

        /// <summary>在命中点播放爆炸特效。</summary>
        /// <param name="center">爆炸中心（子弹命中点）。</param>
        /// <param name="aoeRadius">实际范围半径（0 = 单体模式）。</param>
        /// <param name="config">子弹配置（提供预制体和自动销毁时间）。</param>
        public static void Play(Vector3 center, float aoeRadius, ProjectileConfig config)
        {
            // 没配预制体就只做伤害，不报错、不影响流程
            if (config == null || config.explosionPrefab == null)
                return;

            float visualRadius = Mathf.Max(aoeRadius, MinVisualRadius);

            GameObject vfx = Object.Instantiate(config.explosionPrefab, center, Quaternion.identity);

            if (VfxReferenceRadius > 0f)
                vfx.transform.localScale = Vector3.one * (visualRadius / VfxReferenceRadius);

            // 自动销毁，避免特效对象越积越多（0 表示不自动销毁，由预制体自己控制）
            if (config.explosionVfxLifetime > 0f)
                Object.Destroy(vfx, config.explosionVfxLifetime);
        }
    }
}