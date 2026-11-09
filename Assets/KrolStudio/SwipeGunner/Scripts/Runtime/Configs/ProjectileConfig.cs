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

        // ==================== [范围伤害] 开始 ====================
        // 范围伤害是独立于"爆炸/燃烧/冰冻"的一层：它只决定"一次命中作用于几个敌人"。
        //   AoeRadius <= 0 → 所有效果都只打直击目标（单体 build）
        //   AoeRadius >  0 → 所有效果作用于半径内的全部敌人
        // 增益卡通过 RunBuffService.AddAoeRadius 在局内加它。
        // ========================================================

        [Header("Aoe / 范围伤害")]
        [Tooltip("范围半径（米）。0 = 没有范围技能（所有效果只打直击目标）。")]
        public float aoeRadius = 2f;

        // ==================== [爆炸效果] 开始 ====================
        // 爆炸 = "额外伤害"，不再是范围伤害本身（范围由上面的 aoeRadius 提供）。
        // 没有范围技能时，爆炸变成"直击双倍"，所以它单独拿也有用。
        // ========================================================

        [Header("Explosion / 爆炸")]
        [Tooltip("对直击目标的额外倍率：1 = 再打一份（连同直击伤害合计 2 倍），0 = 只有伤害没有爆炸。")]
        public float explosionDirectExtra = 1f;

        [Tooltip("对范围内其他目标的伤害倍率（还会再乘距离衰减）。")]
        public float explosionDamageMultiplier = 0.5f;

        [Tooltip("边缘伤害比例：1 = 整个范围都吃满伤害；0.3 = 半径边缘只吃 30%。")]
        [Range(0f, 1f)]
        public float explosionFalloff = 0.3f;

        [Tooltip("爆炸特效预制体。建议用纯粒子预制体，不要带 Rigidbody/Collider，否则会干扰物理。")]
        public GameObject explosionPrefab;

        [Tooltip("特效自动销毁时间（秒），0 表示不自动销毁。")]
        public float explosionVfxLifetime = 2f;

        // ==================== [爆炸效果] 结束 ====================

        // ==================== [燃烧效果] 开始 ====================
        // 持续伤害（DoT）：命中后让目标在若干秒内持续掉血（燃烧弹头）。
        // 作用对象由范围伤害决定：没有范围就打直击目标，有范围就点燃一片。
        // 三个值任意一个 <= 0 就不会触发燃烧，等于自带开关（由 RunBuffService 局内改写）。
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

        // ==================== [冰冻效果] 开始 ====================
        // 冻结：命中后让敌人原地停住一段时间（不移动、不转身、不攻击、动画停在当前帧）。
        // 作用对象同样由范围伤害决定；范围内所有敌人统一冻结时长（不衰减）。
        // freezeDuration <= 0 就不会触发，等于自带开关（由 RunBuffService 局内改写）。
        // 目前只对 Stickman 生效，触手不吃（见 FreezeEffect）。
        // ========================================================

        [Header("Freeze / 冰冻")]
        [Tooltip("冻结时长（秒）。0 = 不触发冰冻。")]
        public float freezeDuration = 1.5f;

        [Tooltip("冻结时替换敌人身体材质的冰蓝材质（M_EnemyFrozen）。留空则只有行为冻结、外观不变。")]
        public Material freezeMaterial;

        // ==================== [冰冻效果] 结束 ====================
    }
}