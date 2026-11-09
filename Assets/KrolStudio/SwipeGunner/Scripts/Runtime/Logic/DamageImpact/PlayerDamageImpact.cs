using DG.Tweening;
using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;

namespace KrolStudio
{
    public class PlayerDamageImpact : DamageImpact
    {
        [SerializeField] PlayerHealthBar playerHealthBar;
        [SerializeField] PlayerFx playerFx;

        PlayerHealth playerHealth;
        
        PlayerConfig config;
        IPlayerContext playerContext;
        IInteractorPoolContainer interactorPool;
        Vibration vibration;
        PlayerStateMachine playerStateMachine;

        [Inject]
        void Construct(PlayerConfig config, 
                       IPlayerContext playerContext, 
                       IInteractorPoolContainer interactorPool,
                       Vibration vibration,
                       PlayerStateMachine playerStateMachine)
        {
            this.config = config;
            this.playerContext = playerContext;
            this.interactorPool = interactorPool;
            this.vibration = vibration;
            this.playerStateMachine = playerStateMachine;
        }

        public override void Awake()
        {
            playerHealth = GetComponent<PlayerHealth>();
            base.Awake();
        }

        protected override IHealth Health => playerHealth;

        protected override void OnHealthChange(object sender, System.EventArgs e)
        {
            DamageEventArgs args = e as DamageEventArgs;

            if (args == null)
            {
                Debug.LogError("Invalid EventArgs", this);
                return;
            }

            base.OnHealthChange(sender, e);

            playerContext.DamageFlashEffect?.FlashWhite();

            vibration.Vibrate(GameConstants.Vibrations.ShortVibration);

            playerContext.AudioManager.PlaySound(GameConstants.Sounds.CarDamage);

            if (playerHealthBar != null)
            {
                interactorPool.GetPool<DamagePopup>()
                        .Get(playerHealthBar.DamageTextPos.position)
                        .Play(args.damage, new Color(212f / 255f, 60f / 255f, 59f / 255f, 1f));
            }
            else
            {
                Debug.LogWarning($"{nameof(playerHealthBar)} is null. Please check the reference in the {nameof(PlayerDamageImpact)} component.");
            }

            if (Health.Ratio < config.lowHealthThreshold)
            {
                playerFx?.PlaySpikyFireBig(true);
            }

            DOTween.Complete(transform);
            transform.DOPunchScale(config.punchSettings.GetRandomPunch(), config.punchSettings.duration, config.punchSettings.vibrato, config.punchSettings.elasticity);
        }

        protected override void OnDied(object sender, System.EventArgs e)
        {
            base.OnDied(sender, e);

            playerContext.AudioManager.PlaySound(GameConstants.Sounds.CarCrash);

            playerContext.PlayerFx.PlayNukeExplosion(true);

            Physics.gravity = new Vector3(0f, -35f, 0f);

            playerContext.DamageFlashEffect?.CancelFlashWhite();
            playerContext.ExplosionEffect.Explode();

            playerStateMachine.Enter<PlayerDeadState>().Forget();
        }

        private void OnDisable()
        {
            playerFx.PlaySpikyFireBig(false);
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                DOTween.Complete(transform);
                transform.DOPunchScale(config.punchSettings.GetRandomPunch(), config.punchSettings.duration, config.punchSettings.vibrato, config.punchSettings.elasticity);
            }
        }
#endif
    }
}