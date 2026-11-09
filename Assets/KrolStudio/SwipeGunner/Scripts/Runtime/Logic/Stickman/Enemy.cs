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

        /// <summary>
        /// 是否处于冰冻状态。
        /// 供 EnemyHealth 判断：冻结期间的伤害不要触发受击状态
        /// （否则受击动画在 Animator.speed = 0 时永远播不完，敌人会卡在受击状态）。
        /// </summary>
        bool IsFrozen { get; }

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

        /// <summary>是否处于冰冻状态（由 FreezeEffect 通过 SetFrozen 控制）。</summary>
        bool isFrozen;

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
        public bool IsFrozen => isFrozen;


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

            // 池子复用：冻结状态必须在这里兜底复位。
            // FreezeEffect.OnDisable 也会解冻，但那是另一个组件，不能保证执行顺序，
            // 而且 Animator 在子物体上、不随 Enemy 重置，所以这里显式恢复一次。
            isFrozen = false;
            if (Animator != null)
                Animator.SetSpeed(1f);

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
            // 冻结期间不推进状态机：不移动、不转身、不看追击条件、不进攻击。
            // 攻击伤害是靠动画事件触发的，而冻结时 Animator.speed = 0，事件不会触发，
            // 所以"停状态机 + 停动画"两件事合起来才等于完全冻住。
            //
            // 条件里必须带 Current > 0：EnemyDeadState 也靠这个 Update 驱动
            // （离开视野回收、玩家靠近时推一把），无条件早退会导致尸体永远留在场上。
            if (isFrozen && enemyHealth.Current > 0)
                return;

            enemyStateMachine.UpdateState();
        }

        /// <summary>
        /// 冻结开关。冻结 = 停状态机推进 + 动画速度归零；解冻 = 恢复速度并让状态机继续跑。
        ///
        /// 解冻是"原地继续"而不是"回到初始状态"：
        /// 敌人会从冻住那一刻的状态接着走（正在攻击就接着攻击），
        /// 这样冻结更像"时间静止"，不会因为解冻而回满血或强制回 Idle。
        /// </summary>
        /// <param name="value">true = 冻结，false = 解冻</param>
        public void SetFrozen(bool value)
        {
            // 幂等：重复调用不会产生副作用（FreezeEffect 里会重复调用来兜住边界情况）
            if (isFrozen == value)
                return;

            isFrozen = value;

            // 动画停了，攻击/受击/移动的表现就都停了
            if (Animator != null)
                Animator.SetSpeed(value ? 0f : 1f);
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