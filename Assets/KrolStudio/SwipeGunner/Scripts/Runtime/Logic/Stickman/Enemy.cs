using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public interface IEnemyContext
    {
        EnemyAnimator Animator { get; }
        IHealth Health { get; }
        EnemyStats EnemyStats { get; }

        Enemy ThisEnemy { get; }
        Transform Transform { get; }
        DamageFlashEffect DamageFlashEffect { get; }

        Vector3 SpawnPosition { get; set; }
        AudioManager AudioManager { get; }

        void EnableRagdoll(bool value);
        void Push(Transform playerTransform, float forceMagnitude);
        void Push(Vector3 direction, float forceMagnitude);
        void DisplayShadow(bool value);
    }

    [RequireComponent(typeof(EnemyHealth))]
    [RequireComponent(typeof(EnemyStats))]
    [RequireComponent(typeof(EnemyDamageImpact))]
    public class Enemy : MonoBehaviour, IEnemyContext, ITargetFocus, IPoolReturnable<Enemy>
    {
        [SerializeField] EnemyHealthBar healthBar;
        [SerializeField] AudioManager audioManager;
        [SerializeField] GameObject centerFocus;
        [SerializeField] GameObject Shadow;

        EnemyHealth enemyHealth;
        EnemyStats enemyStats;
        IEnemyGraphicsContext enemyGraphicsContext;

        EnemyStateMachine enemyStateMachine;
        IPersistentProgressService progressService;
        ILogService logService;

        Vector3 spawnPosition;

        public event Action<Enemy> OnReturned;
        
        public IHealth Health => enemyHealth;
        public Transform Transform => transform;
        public Vector3 SpawnPosition { get => spawnPosition; set => spawnPosition = value; }
        public AudioManager AudioManager => audioManager;
        public EnemyStats EnemyStats => enemyStats;
        public bool IsAlive => enemyHealth.Current > 0;
        public Enemy ThisEnemy => this;
        public EnemyAnimator Animator => enemyGraphicsContext.EnemyAnimator;
        public DamageFlashEffect DamageFlashEffect => enemyGraphicsContext.DamageFlashEffect;


        [Inject]
        public void Construct(EnemyStateMachine enemyStateMachine, 
                              IPersistentProgressService progressService,
                              ILogService logService)
        {
            this.enemyStateMachine = enemyStateMachine;
            this.progressService = progressService;
            this.logService = logService;
        }

        void Awake()
        {
            enemyHealth = GetComponent<EnemyHealth>();
            enemyStats = GetComponent<EnemyStats>();

            enemyGraphicsContext = GetComponentInChildren<EnemyGraphicsContext>();

            audioManager.InitSounds();

            healthBar.Initialize(enemyHealth);
            enemyStats.Initialize(progressService.Progress.CurrentLevelIndex, enemyHealth);
            enemyHealth.Initialize();
        }

        void OnEnable()
        {
            gameObject.layer = LayerMask.NameToLayer(GameConstants.Gameplay.EnemyTag); // In EnemyDamageImpact, reset the layer for proper RocketLauncher functionality

            enemyHealth.ResetHealth();
            healthBar.UpdateHealthInstant();
            healthBar.Hide();

            DisplayFocus(false);
            DisplayShadow(true);
            EnableRagdoll(false);

            IdleStateTransition();
        }

        void Update()
        {
            enemyStateMachine.UpdateState();
        }

        void IdleStateTransition()
        {
            enemyStateMachine.Enter<EnemyIdleState>().Forget();
        }

        public void DisplayFocus(bool value)
        {
            centerFocus.gameObject.SetActive(value);
        }

        public void EnableRagdoll(bool value)
        {
            if(enemyGraphicsContext.RagdollHolder != null)
            {
                enemyGraphicsContext.RagdollHolder.gameObject.SetActive(value);
                Animator.Enable(!value);
            }
            else
            {
                logService.LogWarning("ragdollHolder is NULL");
            }
        }

        public void Push(Vector3 direction, float forceMagnitude)
        {
            enemyGraphicsContext.RagdollHolder.linearVelocity = direction * forceMagnitude;
        }

        public void Push(Transform playerTransform, float forceMagnitude)
        {
            Side side = GetTargetSide(playerTransform, transform.position);

            if (side == Side.Left)
            {
                enemyGraphicsContext.RagdollHolder.linearVelocity = playerTransform.right * forceMagnitude;
                //Debug.Log("Enemy on the left");
            }
            else if (side == Side.Right)
            {
                enemyGraphicsContext.RagdollHolder.linearVelocity = -playerTransform.right * forceMagnitude;
                //Debug.Log("Enemy on the right");
            }
            else
            {
                enemyGraphicsContext.RagdollHolder.linearVelocity = UnityEngine.Random.value < 0.5f ? playerTransform.right : -playerTransform.right * forceMagnitude * forceMagnitude;
                //Debug.Log("Enemy on the ahead");
            }
        }
        public Side GetTargetSide(Transform observer, Vector3 targetPosition)
        {
            Vector3 forward = observer.forward;
            forward.y = 0;

            Vector3 toTarget = targetPosition - observer.position;
            toTarget.y = 0;

            Vector3 left = Quaternion.AngleAxis(90, Vector3.up) * forward;

            float dot = Vector3.Dot(left.normalized, toTarget.normalized);

            if (dot > 0.01f)
                return Side.Left;
            else if (dot < -0.01f)
                return Side.Right;
            else
                return Side.Ahead;
        }

        public void ReturnToPool()
        {
            OnReturned?.Invoke(this);
        }

        public void DisplayShadow(bool value)
        {
            Shadow.SetActive(value);
        }
    }
}