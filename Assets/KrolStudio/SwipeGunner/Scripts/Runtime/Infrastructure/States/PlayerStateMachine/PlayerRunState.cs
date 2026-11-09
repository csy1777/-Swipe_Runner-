using Cysharp.Threading.Tasks;
using Zenject;

namespace KrolStudio
{
    public class PlayerRunState : IState, IUpdatableState
    {
        readonly IInputService inputService;
        readonly IShooter turretShooting;
        readonly ITurretRotator turretRotator;
        readonly ITrajectoryRenderer trajectoryRenderer;
        readonly IPlayerContext playerContext;
        readonly PlayerStateMachine stateMachine;
        readonly EnemyPlacer enemyPlacer;

        [Inject]
        public PlayerRunState(IInputService inputService,
                          IPlayerContext playerContext, 
                          IShooter turretShooting,
                          ITurretRotator turretRotator,
                          ITrajectoryRenderer trajectoryRenderer,
                          PlayerStateMachine stateMachine,
                          EnemyPlacer enemyPlacer)
        {
            this.inputService = inputService;
            this.turretRotator = turretRotator;
            this.turretShooting = turretShooting;
            this.trajectoryRenderer = trajectoryRenderer;
            this.playerContext = playerContext;
            this.stateMachine = stateMachine;
            this.enemyPlacer = enemyPlacer;
        }

        public UniTask Enter()
        {
            turretRotator.Subscribe();
            turretShooting.Subscribe();
            trajectoryRenderer.SetActive(true);

            playerContext.SplineMover.Completed += Finish;

            playerContext.AudioManager.PlaySoundFading(GameConstants.Sounds.RunningCar, 0f, 1f, 1f);

            playerContext.PlayerFx.PlaySmokeDarkTrail(true);
            playerContext.PlayerFx.PlayWheelTrail(true);

            return default;
        }


        public void Update()
        {
            playerContext.SplineMover.UpdatePosition();
            playerContext.RocketLauncher.Tick();
            enemyPlacer.Tick();
            turretShooting.ShotUpdate();
            trajectoryRenderer.RenderLaser();
        }

        public void Finish()
        {
            stateMachine.Enter<PlayerFinishState>().Forget();
        }

        public UniTask Exit()
        {
            playerContext.RocketLauncher.DisableFocus();

            playerContext.SplineMover.Completed -= Finish;

            turretRotator.Unsubscribe();
            turretShooting.Unsubscribe();
            trajectoryRenderer.SetActive(false);

            playerContext.AudioManager.StopSoundFading(GameConstants.Sounds.RunningCar, 1f, 0f, 0.5f);

            playerContext.PlayerFx.PlaySmokeDarkTrail(false);
            playerContext.PlayerFx.PlayWheelTrail(false);
            
            return default;
        }
    }
}
