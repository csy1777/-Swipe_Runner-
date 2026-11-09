using Cysharp.Threading.Tasks;
using Zenject;

namespace KrolStudio
{
    public class WinGameState : IState
    {
        readonly IWindowService windowService;
        readonly ILevelRewardService levelRewardService;

        [Inject]
        public WinGameState(IWindowService windowService,
                            ISaveLoadService saveLoadService,
                            ILevelRewardService levelRewardService)
        {
            this.windowService = windowService;
            this.levelRewardService = levelRewardService;
        }

        public UniTask Enter()
        {
            // ==== [关卡结算] ====
            // 先结算奖励（用"刚打完的这一关"的编号），再打开结算界面。
            // 关卡推进由 CompleteWindow.OnEnable → IncrementLevelIndex 负责，两者分开，
            // 避免"先 +1 再算奖励"导致奖励关判定错位。
            // ==== [关卡结算] end ====
            levelRewardService.AwardLevelClear();

            windowService.Open(WindowType.CompleteScreen);
            return default;
        }

        public UniTask Exit()
        {
            return default;
        }
    }
}