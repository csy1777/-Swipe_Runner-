using Cysharp.Threading.Tasks;
using System;
using Zenject;

namespace KrolStudio
{
    public class TutorialState : IState
    {
        readonly IWindowService windowService;
        readonly ILogService logService;
        readonly GameplayStateMachine gameplayStateMachine;
        readonly CameraSwitcher cameraSwitcher;
        readonly IHudView hudView;
        readonly IRocketHud rocketHud;
        readonly IOverHeatHudView overHeatHud;
        readonly IPlayerContext playerContext;
        readonly SlideMoverContext slideMoverContext;

        [Inject]
        public TutorialState(GameplayStateMachine gameplayStateMachine, 
                             ILogService logService,
                             IWindowService windowService,
                             CameraSwitcher cameraSwitcher,
                             IHudView hudView,
                             IRocketHud rocketHud,
                             IOverHeatHudView overHeatHud,
                             IPlayerContext playerContext,
                             SlideMoverContext slideMoverContext)
        {
            this.gameplayStateMachine = gameplayStateMachine;
            this.logService = logService;
            this.windowService = windowService;
            this.cameraSwitcher = cameraSwitcher;
            this.hudView = hudView;
            this.rocketHud = rocketHud;
            this.overHeatHud = overHeatHud;
            this.playerContext = playerContext;
            this.slideMoverContext = slideMoverContext;
        }

        public async UniTask Enter()
        {
            logService.Log($"Entered {nameof(TutorialState)}");

            await slideMoverContext.StartSlide();

            hudView.DisplayMeterProgress(true);
            rocketHud.DisplayRocketLauncher(true);
            overHeatHud.DisplayOverHeat(true);
            playerContext.UpgradeLevel.DisplayLevelIndicators(false);

            await UniTask.Delay(TimeSpan.FromSeconds(0.5d), ignoreTimeScale: false);

            cameraSwitcher.SwitchToFollow();
            windowService.Open(WindowType.TutorialScreen);
        }

        public UniTask Exit()
        {
            return default;
        }
    }
}