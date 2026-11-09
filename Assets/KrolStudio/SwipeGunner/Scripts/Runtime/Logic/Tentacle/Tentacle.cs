using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public interface ITentacleContext
    {
        IHealth Health { get; }
        TentacleStats TentacleStats { get; }
        TentacleAnimator TentacleAnimator { get; }
        DamageFlashEffect DamageFlashEffect { get; }
        Transform Transform { get; }
        AudioManager AudioManager { get; }
    }

    public interface ITargetFocus
    {
        Transform Transform { get; }
        void DisplayFocus(bool value);
    }

    [RequireComponent(typeof(TentacleHealth))]
    [RequireComponent(typeof(TentacleStats))]
    [RequireComponent(typeof(TentacleDamageImpact))]
    public class Tentacle : MonoBehaviour, ITentacleContext, ITargetFocus, IPoolReturnable<Tentacle>
    {
        [SerializeField] EnemyHealthBar healthBar;
        [SerializeField] AudioManager audioManager;
        [SerializeField] GameObject centerFocus;

        TentacleHealth tentacleHealth;
        TentacleStats tentacleStats;
        ITentacleGraphicsContext tentacleGraphicsContext;

        TentacleStateMachine tentacleStateMachine;
        IPersistentProgressService progressService;

        public event Action<Tentacle> OnReturned;

        public DamageFlashEffect DamageFlashEffect => tentacleGraphicsContext.DamageFlashEffect;
        public TentacleAnimator TentacleAnimator => tentacleGraphicsContext.TentacleAnimator;
        public Transform Transform => transform;
        public IHealth Health => tentacleHealth;
        public TentacleStats TentacleStats => tentacleStats;
        public AudioManager AudioManager => audioManager;
        public bool IsAlive => tentacleHealth.Current > 0;

        [Inject]
        public void Construct(TentacleStateMachine tentacleStateMachine, IPersistentProgressService progressService)
        {
            this.tentacleStateMachine = tentacleStateMachine;
            this.progressService = progressService;
        }

        void Awake()
        {
            tentacleHealth = GetComponent<TentacleHealth>();
            tentacleStats = GetComponent<TentacleStats>();
            tentacleGraphicsContext = GetComponentInChildren<TentacleGraphicsContext>();

            audioManager.InitSounds();

            healthBar.Initialize(tentacleHealth);
            tentacleStats.Initialize(progressService.Progress.CurrentLevelIndex, tentacleHealth);
            tentacleHealth.Initialize();
        }

        void OnEnable()
        {
            gameObject.layer = LayerMask.NameToLayer(GameConstants.Gameplay.EnemyTag); // To make the Projectile respond

            DisplayFocus(false);
            tentacleHealth.ResetHealth();
            healthBar.UpdateHealthInstant();
            healthBar.Hide();

            IdleStateTransition();
        }

        void Update()
        {
            tentacleStateMachine.UpdateState();
        }

        void IdleStateTransition()
        {
            tentacleStateMachine.Enter<TentacleIdleState>().Forget();
        }

        public void DisplayFocus(bool value)
        {
            centerFocus.gameObject.SetActive(value);
        }

        public void ReturnToPool()
        {
            OnReturned?.Invoke(this);
        }
    }
}