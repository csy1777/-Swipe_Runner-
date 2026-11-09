namespace KrolStudio
{
    /// <summary>
    /// 一局（run）内的子弹数值快照：SO_ProjectileConfig 里的基础值 + 本局增益卡的结果。
    ///
    /// 为什么需要它：
    ///   1) 效果不再直接读 ProjectileConfig —— 那是全局共享资产，运行期改它会污染所有对局；
    ///      更坑的是编辑器退出 Play 会自动回滚，导致"编辑器里看着对、打包后错"；
    ///   2) 卡牌系统需要一个地方叠加数值（阶段 5）；
    ///   3) 失败时需要一个地方一键清空（LoseGameState → RunBuffService.Reset）。
    ///
    /// 只放"数值"；粒子预制体、材质这类资产仍然放在 ProjectileConfig 里。
    /// </summary>
    public struct BulletStats
    {
        /// <summary>
        /// 范围伤害半径（米）。
        /// 0 = 没有范围技能：所有效果（爆炸/燃烧/冰冻）都只作用于直击目标。
        /// 增益卡通过 RunBuffService 改它。
        /// </summary>
        public float AoeRadius;

        /// <summary>
        /// 爆炸对"直击目标"的额外倍率：1 = 再打一份伤害（连同直击伤害合计 2 倍），0 = 没有爆炸加成。
        /// 注意它是在直击伤害之外额外结算的，不是替换。
        /// </summary>
        public float ExplosionDirectExtra;

        /// <summary>爆炸对范围内其他目标的伤害倍率（还要再乘距离衰减）。</summary>
        public float ExplosionDamageMultiplier;

        /// <summary>爆炸边缘伤害比例：1 = 全范围都吃满，0.3 = 半径边缘只吃 30%。</summary>
        public float ExplosionFalloff;

        /// <summary>每秒燃烧伤害。0 = 没有燃烧。</summary>
        public float BurnDamagePerSecond;

        /// <summary>燃烧持续时间（秒）。0 = 没有燃烧。</summary>
        public float BurnDuration;

        /// <summary>燃烧每隔多少秒跳一次伤害。</summary>
        public float BurnTickInterval;

        /// <summary>冰冻时长（秒）。0 = 没有冰冻。范围内所有敌人统一时长。</summary>
        public float FreezeDuration;
    }
}