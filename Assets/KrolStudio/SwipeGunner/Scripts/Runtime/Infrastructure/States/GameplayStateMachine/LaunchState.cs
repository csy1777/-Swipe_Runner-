using Cysharp.Threading.Tasks;
using Zenject;

namespace KrolStudio
{
    public class LaunchState : IState
    {
        readonly IWindowService windowService;
        readonly ILogService logService;
        readonly GameplayStateMachine gameplayStateMachine;
        readonly CameraSwitcher cameraSwitcher;
        readonly IHudView hudView;
        readonly IRocketHud rocketHud;
        readonly IOverHeatHudView overHeatHud;
        readonly ISaveLoadService saveLoadService;
        readonly IPlayerContext playerContext;

        [Inject]
        public LaunchState(GameplayStateMachine gameplayStateMachine,
                           ILogService logService,
                           IWindowService windowService,
                           CameraSwitcher cameraSwitcher,
                           IHudView hudView,
                           IRocketHud rocketHud,
                           IOverHeatHudView overHeatHud,
                           ISaveLoadService saveLoadService,
                           IPlayerContext playerContext)
        {
            this.gameplayStateMachine = gameplayStateMachine;
            this.logService = logService;
            this.windowService = windowService;
            this.cameraSwitcher = cameraSwitcher;
            this.hudView = hudView;
            this.rocketHud = rocketHud;
            this.overHeatHud = overHeatHud;
            this.saveLoadService = saveLoadService;
            this.playerContext = playerContext;
        }

        public UniTask Enter()
        {
            logService.Log($"Entered {nameof(LaunchState)}");

            hudView.DisplayMeterProgress(false);
            rocketHud.DisplayRocketLauncher(false);
            overHeatHud.DisplayOverHeat(false);
            

            cameraSwitcher.SwitchToStart();

            windowService.Open(WindowType.LaunchScreen);

            return default;
        }

        public UniTask Exit()
        {
            saveLoadService.SaveProgress(SaveKey.All);     // For saving the upgrade and other related data
            return default;
        }
    }
}