using Cysharp.Threading.Tasks;
using Zenject;

namespace KrolStudio
{
    public class LoseGameState : IState
    {
        readonly IWindowService windowService;
        readonly IRunBuffService runBuffService;

        [Inject]
        public LoseGameState(IWindowService windowService, IRunBuffService runBuffService)
        {
            this.windowService = windowService;
            this.runBuffService = runBuffService;
        }

        public UniTask Enter()
        {
            // ==== [双层进度] 失败：局内层清零 ====
            // 设计是"失败：卡清零、芯片返还 30%、关卡进度与零件保留"。
            // 局内数值全部由 RunBuffService 承担，所以这里一行就能清干净；
            // 关卡进度 / 零件等级在存档（PlayerProgress）里，不受影响。
            // ==== [双层进度] end ====
            runBuffService.Reset();

            windowService.Open(WindowType.FailScreen);
            return default;
        }

        public UniTask Exit()
        {
            return default;
        }
    }
}