namespace KrolStudio
{
    /// <summary>
    /// 本局（run）的子弹数值服务。
    ///
    /// 住在 ProjectContext（DontDestroyOnLoad）：跨关卡保留、失败时 Reset。
    /// 效果只读 <see cref="Stats"/>，不再直接读 ProjectileConfig 的数值。
    /// </summary>
    public interface IRunBuffService
    {
        /// <summary>当前这一局的数值快照。只读，改数值要走下面的方法。</summary>
        BulletStats Stats { get; }

        /// <summary>回到本局初始状态（清空所有卡牌加成，重新读 baseline）。</summary>
        void Reset();

        /// <summary>直接设定范围半径（调试用）。</summary>
        void SetAoeRadius(float radius);

        /// <summary>叠加范围半径（增益卡用）。</summary>
        void AddAoeRadius(float value);
    }
}