using UnityEngine;
using Zenject;

namespace KrolStudio
{
    /// <summary>
    /// 本局数值服务。
    ///
    /// 为什么绑在 ProjectContext（GameInstaller）而不是 GameplayInstaller：
    ///   每关结束都会 sceneLoader.Load("Level") 重载整个场景，场景级服务会被重建。
    ///   而"一局 = 多关"，局内 buff 必须跨关保留，所以只能放在全局容器里。
    ///   ProjectContext 是 DontDestroyOnLoad，天然满足。
    ///
    /// 为什么不写 ProjectileConfig：
    ///   那是 ScriptableObject 资产，全局唯一。运行期改它会在所有对局之间互相污染，
    ///   而且编辑器退出 Play 会把改动回滚 —— 编辑器里测不出来，打包后才炸。
    ///   所以这里只读 baseline，把结果放进本地的 BulletStats。
    /// </summary>
    public class RunBuffService : IRunBuffService
    {
        /// <summary>只读基准值来源（SO_ProjectileConfig）。</summary>
        readonly ProjectileConfig baseline;

        /// <summary>本局当前数值。Reset 时从 baseline 重新构造。</summary>
        BulletStats stats;

        [Inject]
        public RunBuffService(ProjectileConfig baseline)
        {
            this.baseline = baseline;

            // 冷启动 / 新一局：从基准值开始
            Reset();
        }

        public BulletStats Stats => stats;

        /// <summary>
        /// 回到本局初始状态：所有卡牌加成清零，数值重新取 baseline。
        /// 失败时调用（LoseGameState），跨关通关时不要调用。
        /// </summary>
        public void Reset()
        {
            stats = new BulletStats
            {
                AoeRadius = baseline.aoeRadius,
                ExplosionDirectExtra = baseline.explosionDirectExtra,
                ExplosionDamageMultiplier = baseline.explosionDamageMultiplier,
                ExplosionFalloff = baseline.explosionFalloff,
                BurnDamagePerSecond = baseline.burnDamagePerSecond,
                BurnDuration = baseline.burnDuration,
                BurnTickInterval = baseline.burnTickInterval,
                FreezeDuration = baseline.freezeDuration,
            };
        }

        /// <summary>直接设定范围半径。传 0 就是"没有范围技能"（所有效果只打直击目标）。</summary>
        public void SetAoeRadius(float radius)
        {
            stats.AoeRadius = Mathf.Max(0f, radius);
        }

        /// <summary>在现有半径上叠加（增益卡 +0.5m 之类）。</summary>
        public void AddAoeRadius(float value)
        {
            SetAoeRadius(stats.AoeRadius + value);
        }
    }
}