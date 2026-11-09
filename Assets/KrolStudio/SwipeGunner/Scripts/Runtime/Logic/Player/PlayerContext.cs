using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public interface IPlayerContext
    {
        PlayerChaseTarget PlayerChaseTarget { get; }
        PlayerHealth PlayerHealth { get; }
        SplineMover SplineMover { get; }
        PlayerFx PlayerFx { get; }
        PlayerStats PlayerStats { get; }
        AmmoTracker AmmoTracker { get; }
      
        PlayerHealthBar PlayerHealthBar { get; }
        AudioManager AudioManager { get; }
        BoardManager BoardManager { get; }
        Transform Transform { get; }
        Transform CameraLookAtTarget { get; }

        UpgradeLevelController UpgradeLevel { get; }
        ExplosionEffect ExplosionEffect { get; }
        RocketLauncher RocketLauncher { get; }
        Turret Turret { get; }
        DamageFlashEffect DamageFlashEffect { get; }
        Collider PlayerGraphicsCollider { get; }
    }

    [RequireComponent(typeof(PlayerHealth))]
    [RequireComponent(typeof(SplineMover))]
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(AmmoTracker))]
    [RequireComponent(typeof(PlayerChaseTarget))]
    [RequireComponent(typeof(PlayerDamageImpact))]
    public class PlayerContext : MonoBehaviour, ITickable, IPlayerContext
    {
        PlayerHealth playerHealth;
        SplineMover splineMover;
        PlayerStats playerStats;
        AmmoTracker ammoTracker;
        PlayerChaseTarget playerChaseTarget;

        IPlayerGraphicsContext playerGraphicsContext;

        [SerializeField] PlayerHealthBar playerHealthBar;
        [SerializeField] PlayerFx playerFx;
        [SerializeField] AudioManager audioManager;
        [SerializeField] BoardManager boardManager;
        [SerializeField] Transform cameraLookAtTarget;

        public PlayerChaseTarget PlayerChaseTarget => playerChaseTarget;
        public PlayerHealth PlayerHealth => playerHealth;
        public SplineMover SplineMover => splineMover;
        public PlayerStats PlayerStats => playerStats;
        public AmmoTracker AmmoTracker => ammoTracker;

        public ExplosionEffect ExplosionEffect => playerGraphicsContext.ExplosionEffect;
        public RocketLauncher RocketLauncher => playerGraphicsContext.RocketLauncher;
        public Turret Turret => playerGraphicsContext.Turret;
        public UpgradeLevelController UpgradeLevel => playerGraphicsContext.UpgradeLevel;
        public DamageFlashEffect DamageFlashEffect => playerGraphicsContext.DamageFlashEffect;
        public Collider PlayerGraphicsCollider => playerGraphicsContext.PlayerGraphicsCollider;

        public PlayerFx PlayerFx => playerFx;
        public PlayerHealthBar PlayerHealthBar => playerHealthBar;
        public AudioManager AudioManager => audioManager;   
        public BoardManager BoardManager => boardManager;
        public Transform Transform => transform;
        public Transform CameraLookAtTarget => cameraLookAtTarget;

        IStatesFactory statesFactory;
        PlayerStateMachine stateMachine;

        [Inject]
        void Construct(PlayerStateMachine stateMachine, IStatesFactory statesFactory)
        {
            this.statesFactory = statesFactory;
            this.stateMachine = stateMachine;

            stateMachine.SetStatesFactory(statesFactory);
        }

        void Awake()
        {
            playerHealth = GetComponent<PlayerHealth>();
            splineMover = GetComponent<SplineMover>();
            playerStats = GetComponent<PlayerStats>();
            ammoTracker = GetComponent<AmmoTracker>();
            playerChaseTarget = GetComponent<PlayerChaseTarget>();

            playerGraphicsContext = GetComponentInChildren<PlayerGraphicsContext>();

            playerHealthBar.Initialize(playerHealth);
            audioManager.InitSounds();
        }

        public void Tick()
        {
            stateMachine.UpdateState();
        }
    }
}