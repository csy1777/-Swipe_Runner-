using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class TentacleDamageImpact : DamageImpact
    {
        [SerializeField] EnemyHealthBar healthBar;
        [SerializeField] EnemyFx enemyFx;

        IWalletService walletService;
        IInteractorPoolContainer interactorPool;
        ITentacleContext tentacleContext;
        UpgradeSystem upgradeSystem;
        TentacleStateMachine tentacleStateMachine;

        TentacleHealth enemyHealth;

        protected override IHealth Health => enemyHealth;

        [Inject]
        void Construct(IInteractorPoolContainer interactorPool,
                       IWalletService walletService,
                       ITentacleContext tentacleContext,
                       UpgradeSystem upgradeSystem,
                       TentacleStateMachine tentacleStateMachine)
        {
            this.interactorPool = interactorPool;
            this.walletService = walletService;
            this.tentacleContext = tentacleContext;
            this.upgradeSystem = upgradeSystem;
            this.tentacleStateMachine = tentacleStateMachine;
        }

        public override void Awake()
        {
            enemyHealth = GetComponent<TentacleHealth>();
            base.Awake();
        }

        //#if UNITY_EDITOR
        //        private void Update()
        //        {
        //            if (Input.GetKeyDown(KeyCode.D))
        //            {
        //                interactorPool.GetPool<DamagePopup>()
        //                                .Get(healthBar.transform.position)
        //                                .Play(32, Color.white);
        //            }
        //        }
        //#endif

        protected override void OnHealthChange(object sender, EventArgs e)
        {
            DamageDirectionEventArgs args = e as DamageDirectionEventArgs;

            if (args == null)
            {
                Debug.LogError("Invalid EventArgs", this);
                return;
            }

            base.OnHealthChange(sender, e);

            tentacleContext.DamageFlashEffect?.FlashWhite();

            interactorPool.GetPool<DamagePopup>()
                          .Get(healthBar.DamageTextPos.position)
                          .Play(args.damage, Color.white);
        }

        protected override void OnDied(object sender, EventArgs e)
        {
            base.OnDied(sender, e);

            tentacleContext.AudioManager.PlaySound(GameConstants.Sounds.TentacleDeath);

            (tentacleContext as ITargetFocus).DisplayFocus(false);

            tentacleContext.Transform.gameObject.layer = LayerMask.NameToLayer(GameConstants.Gameplay.DefaultTag); // So that the Projectile does not trigger

            var args = e as DamageDirectionEventArgs;
            if (args != null)
            {
                enemyFx.PlayBloodEffect(args.direction);
            }

            float killReward = upgradeSystem.GetEnemyKillReward(sender is Enemy);

            interactorPool.GetPool<EnemyKillRewardText>().Get(healthBar.transform.position).Play(NumberFormatter.Format((int)killReward));

            walletService.AddAmount(CurrencyType.Coins, Mathf.RoundToInt(killReward * 10f));

            tentacleStateMachine.Enter<TentacleDeadState>().Forget();
        }

        private async UniTask ExecuteWithDelay(Action action, double delay)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delay));
                action?.Invoke();
            }           
            catch (MissingReferenceException ex)
            {
                Debug.LogWarning($"The object was destroyed during animation: {ex.Message}", this);
            }
            catch (NullReferenceException ex)
            {
                Debug.LogWarning($"Missing link during animation: {ex.Message}", this);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error: {ex}", this);
            }
        }
    }
}