using Cysharp.Threading.Tasks;

namespace KrolStudio
{
    public class PlayerIdleState : IState
    {
        readonly IPlayerContext playerContext;
        readonly ILevelPlacer levelPlacer;
        readonly IHudView hudView;
        readonly CameraSwitcher cameraSwitcher;
        readonly AudioManager globalAudioManager;

        public PlayerIdleState(IPlayerContext playerContext, 
                               CameraSwitcher cameraSwitcher, 
                               ILevelPlacer levelPlacer,
                               IHudView hudView,
                               AudioManager globalAudioManager)
        {
            this.playerContext = playerContext;
            this.levelPlacer = levelPlacer;
            this.hudView = hudView;
            this.cameraSwitcher = cameraSwitcher;
            this.globalAudioManager = globalAudioManager;
        }

        public UniTask Enter()
        {
            globalAudioManager.PlaySound(GameConstants.Sounds.Main);

            playerContext.PlayerHealthBar.Hide();

            cameraSwitcher.Initialize(playerContext.CameraLookAtTarget);

            playerContext.SplineMover.spline = levelPlacer.GetPathSpline;
            playerContext.SplineMover.Restart();
            playerContext.SplineMover.SetPosition();
            playerContext.SplineMover.SetStartSpeed();

            return default;
        }
        
        public UniTask Exit()
        {
            playerContext.PlayerStats.Initialize();
            playerContext.PlayerHealth.Initialize();
            playerContext.PlayerHealthBar.UpdateHealthInstant();

            playerContext.PlayerHealthBar.Show();
            playerContext.AmmoTracker.Initialize(playerContext.PlayerHealthBar.UpdateAmmoUI, hudView.ShowOutOfBulletsMessage);
            
            return default;
        }
    }
}
