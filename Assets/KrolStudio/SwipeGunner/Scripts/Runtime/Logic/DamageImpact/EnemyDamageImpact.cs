using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class EnemyDamageImpact : DamageImpact
    {
        [SerializeField] EnemyHealthBar healthBar;
        [SerializeField] EnemyFx enemyFx;
        
        IWalletService walletService;
        IInteractorPoolContainer interactorPool;
        IEnemyContext enemyContext;
        UpgradeSystem upgradeSystem;
        EnemyConfig enemyConfig;
        EnemyStateMachine enemyStateMachine;

        EnemyHealth enemyHealth;

        protected override IHealth Health => enemyHealth;

        [Inject]
        void Construct(IInteractorPoolContainer interactorPool, 
                       IWalletService walletService,
                       IEnemyContext enemyContext,
                       UpgradeSystem upgradeSystem,
                       EnemyConfig enemyConfig,
                       EnemyStateMachine enemyStateMachine)
        {
            this.interactorPool = interactorPool;
            this.walletService = walletService;
            this.enemyContext = enemyContext;
            this.upgradeSystem = upgradeSystem;
            this.enemyConfig = enemyConfig;
            this.enemyStateMachine = enemyStateMachine;
        }

        public override void Awake()
        {
            enemyHealth = GetComponent<EnemyHealth>();
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

            enemyContext.DamageFlashEffect.FlashWhite();

            enemyContext.AudioManager.PlaySound(GameConstants.Sounds.GetRandomStickmanDamage());

            if(healthBar != null)
            {
                interactorPool.GetPool<DamagePopup>()
                         .Get(healthBar.DamageTextPos.position)
                         .Play(args.damage, Color.white);
            }
            else
            {
                Debug.LogWarning($"{nameof(healthBar)} is null. Please check the reference in the {nameof(EnemyDamageImpact)} component.");
            }
        }

        protected override void OnDied(object sender, EventArgs e)
        {
            base.OnDied(sender, e);

            enemyContext.DisplayShadow(false);
            (enemyContext as ITargetFocus).DisplayFocus(false);
            enemyContext.EnableRagdoll(true);

            enemyContext.Transform.gameObject.layer = LayerMask.NameToLayer(GameConstants.Gameplay.DefaultTag); // To ensure RocketLauncher works correctly

            var args = e as DamageDirectionEventArgs;
            if (args != null)
            {
                enemyContext.Push(args.direction, enemyConfig.deathPushForce);

                if(enemyFx != null)
                    enemyFx.PlayBloodEffect(args.direction);
            }
           
            float killReward = upgradeSystem.GetEnemyKillReward(sender is Enemy);

            if (healthBar != null)
            {
                interactorPool.GetPool<EnemyKillRewardText>()
                              .Get(healthBar.transform.position)
                              .Play(NumberFormatter.Format((int)killReward));
            }
            else
            {
                Debug.LogWarning($"{nameof(healthBar)} is null. Please check the reference in the {nameof(EnemyDamageImpact)} component.");
            }

            walletService.AddAmount(CurrencyType.Coins, Mathf.RoundToInt(killReward * 10f));

            // ==== [芯片] 击杀掉落（普通怪） ====
            // 芯片是局内货币：这里只负责加钱，失败返还 30% 由 LoseGameState 那一侧处理。
            // 数量在 SO_EnemyConfig 里配，方便后面做"精英怪 +3"之类的差异化。
            walletService.AddAmount(CurrencyType.Chips, enemyConfig.chipsPerKill);
            // ==== [芯片] end ====

            enemyStateMachine.Enter<EnemyDeadState>().Forget();
        }
    }
}