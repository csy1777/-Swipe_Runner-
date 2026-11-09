using UnityEngine;

namespace KrolStudio
{
    [CreateAssetMenu(fileName = "ProjectileConfig", menuName = "Configs/ProjectileConfig")]
    public class ProjectileConfig : ScriptableObject
    {
        public float flyTime = 10f;             // projectile lifetime (in seconds)
        public float flySpeed = 15f;
        public float sphereCastRadius = 0.1f;   // radius for collision check (SphereCast)

        [Space]
        public int baseDamage = 5;              // base damage at level 1
        public float damageGrowthPerLevel = 5;  // damage increase per level

        // ==================== [爆炸效果] 开始 ====================
        // 下面是"子弹命中后爆炸"用到的参数。
        // 如果不想要爆炸：把 explosionRadius 设成 0，或在 Projectile.HandleAttack 里
        // 注释掉 ImpactExplosion.Resolve(...) 那一行即可，其余逻辑不受影响。
        // ========================================================

        [Header("Explosion / 爆炸")]
        [Tooltip("爆炸半径（米）。设为 0 表示不产生爆炸。")]
        public float explosionRadius = 2f;

        [Tooltip("爆炸伤害倍率：爆炸伤害 = 当前子弹伤害 × 该值。")]
        public float explosionDamageMultiplier = 0.5f;

        [Tooltip("边缘伤害比例：1 = 整个范围都吃满伤害；0.3 = 半径边缘只吃 30%。")]
        [Range(0f, 1f)]
        public float explosionFalloff = 0.3f;

        [Tooltip("勾选后，被子弹直接命中的目标也会额外吃一次爆炸伤害（会造成双倍伤害）。")]
        public bool explosionAffectsDirectTarget = false;

        [Tooltip("爆炸特效预制体。建议用纯粒子预制体，不要带 Rigidbody/Collider，否则会干扰物理。")]
        public GameObject explosionPrefab;

        [Tooltip("特效自动销毁时间（秒），0 表示不自动销毁。")]
        public float explosionVfxLifetime = 2f;

        // ==================== [爆炸效果] 结束 ====================

        // ==================== [燃烧效果] 开始 ====================
        // 持续伤害（DoT）：命中后让目标在若干秒内持续掉血。
        // 三个值任意一个 <= 0 就不会触发燃烧，等于自带开关。
        // ========================================================

        [Header("Burn / 燃烧")]
        [Tooltip("每秒燃烧伤害。0 = 不触发燃烧。")]
        public float burnDamagePerSecond = 10f;

        [Tooltip("燃烧持续时间（秒）。0 = 不触发燃烧。")]
        public float burnDuration = 3f;

        [Tooltip("每隔多少秒跳一次伤害（例如 0.5 = 每半秒一次）。")]
        public float burnTickInterval = 0.5f;

        [Tooltip("燃烧粒子预制体（VFX_EnemyBurn）。留空则只有伤害、没有火焰表现。")]
        public GameObject burnVfxPrefab;

        [Tooltip("燃烧粒子挂载在目标身上的高度（米），一般取躯干高度。")]
        public float burnVfxHeight = 0.8f;

        // ==================== [燃烧效果] 结束 ====================
    }
}