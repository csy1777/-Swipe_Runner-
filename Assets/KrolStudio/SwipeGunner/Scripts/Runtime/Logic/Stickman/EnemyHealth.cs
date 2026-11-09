using Cysharp.Threading.Tasks;
using System;
using Zenject;

namespace KrolStudio
{
    public class EnemyHealth : Health
    {
        EnemyStateMachine stateMachine;
        EnemyConfig config;
        IEnemyContext enemyContext;

        [Inject]
        void Construct(EnemyStateMachine stateMachine, EnemyConfig config, IEnemyContext enemyContext)
        {
            this.stateMachine = stateMachine;
            this.config = config;
            this.enemyContext = enemyContext;
        }

        public override void Initialize()
        {
            base.Initialize();

            SetMaxHealth(enemyContext.EnemyStats.CurrentHealth);
            ResetHealth();
        }

        public override void TakeDamage(int damage, object sender, EventArgs e)
        {
            base.TakeDamage(damage, sender, e);

            // ==== [燃烧效果] 开始 ====
            // 燃烧的每一跳伤害都不应该把敌人推进受击状态：
            // 否则每 0.5 秒打断一次行为；更糟的是当敌人正处于 Attack/Dead 等状态时，
            // EnemyHitState 找不到"上一个状态"可回，会一直卡在受击状态里。
            // 普通子弹伤害的行为不变（IsBurnTick 只会在燃烧跳伤的那一瞬间为 true）。
            // 如果确实想让燃烧也打断敌人：删掉 !BurnEffect.IsBurnTick 这个条件即可。
            // ==== [冰冻效果] 开始 ====
            // 冻结期间同样不能进受击状态：
            // 冻结时 Animator.speed = 0，受击动画永远播不完，OnExitHitState 不会回调，
            // 敌人会永久卡在 EnemyHitState（和燃烧 DoT 是同一个坑）。
            // ==== [冰冻效果] end ====
            if (Current > 0 && !BurnEffect.IsBurnTick && !enemyContext.IsFrozen)
                stateMachine.Enter<EnemyHitState>().Forget();
            // ==== [燃烧效果] end ====
        }
    }
}