using UnityEngine;
using Zenject;

namespace KrolStudio
{
    /// <summary>
    /// 关卡结算：只负责"打通一关给多少芯片"，不碰关卡进度。
    ///
    /// 为什么和关卡推进分开：
    ///   - 奖励要用"刚打完的这一关"的编号来算（判断是不是奖励关）；
    ///   - 关卡推进（IncrementLevelIndex）在 CompleteWindow 里，那时编号已经 +1；
    ///   放一起很容易算错关，分开后各自只关心自己的事。
    ///
    /// 调用点：WinGameState.Enter()，在打开结算界面之前。
    /// </summary>
    public interface ILevelRewardService
    {
        /// <summary>结算当前这一关的通关奖励（芯片）。重复调用会重复发钱，只应在胜利时调一次。</summary>
        void AwardLevelClear();
    }

    public class LevelRewardService : ILevelRewardService
    {
        readonly IWalletService walletService;
        readonly RewardConfig rewardConfig;
        readonly IPersistentProgressService progressService;
        readonly ILogService logService;

        [Inject]
        public LevelRewardService(IWalletService walletService,
                                  RewardConfig rewardConfig,
                                  IPersistentProgressService progressService,
                                  ILogService logService)
        {
            this.walletService = walletService;
            this.rewardConfig = rewardConfig;
            this.progressService = progressService;
            this.logService = logService;
        }

        public void AwardLevelClear()
        {
            if (progressService.Progress == null)
                return;

            // 注意：这时 CompleteWindow 还没执行 IncrementLevelIndex，
            // 所以 CurrentLevelIndex 就是"刚打完的这一关"。
            int level = progressService.Progress.CurrentLevelIndex;
            if (level <= 0)
                return;

            int chips = rewardConfig.chipsPerLevelClear;

            // 奖励关（默认每 5 关）芯片翻倍
            bool isRewardLevel = rewardConfig.rewardLevelInterval > 0
                                 && level % rewardConfig.rewardLevelInterval == 0;

            if (isRewardLevel)
                chips = Mathf.RoundToInt(chips * rewardConfig.rewardLevelChipsMultiplier);

            if (chips <= 0)
                return;

            walletService.AddAmount(CurrencyType.Chips, chips);

            logService.Log($"Level {level} clear: +{chips} chips" +
                           (isRewardLevel ? $" (reward level x{rewardConfig.rewardLevelChipsMultiplier})" : string.Empty));
        }
    }
}