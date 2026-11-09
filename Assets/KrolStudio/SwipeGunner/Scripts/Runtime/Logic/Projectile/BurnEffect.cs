using UnityEngine;

namespace KrolStudio
{
    /// <summary>
    /// 燃烧（持续伤害 / DoT）+ 燃烧粒子表现。
    ///
    /// 工作方式：
    ///   - 由子弹命中时调用 <see cref="ApplyTo"/> 挂到目标身上（同一个目标只挂一个）；
    ///   - 第一次施加时，把配置里的燃烧粒子实例化到目标身上（跟随目标移动）；
    ///   - 之后每 <c>tickInterval</c> 秒对目标调用一次 <c>IHealth.TakeDamage</c>；
    ///   - 持续时间结束后销毁粒子并自我销毁；
    ///   - 目标被池子回收（SetActive(false)）时同样会清理，不会带到下一只怪身上。
    ///
    /// 数值来源：<see cref="BulletStats"/>（本局快照，会随卡牌变化），
    /// 材质/粒子仍然取自 <see cref="ProjectileConfig"/>（不会变的资产）。
    /// 施加时会把数值拷贝到自己身上，所以之后卡牌再变也不会影响已经点燃的目标。
    /// </summary>
    public class BurnEffect : MonoBehaviour
    {
        /// <summary>被点燃的目标（血量组件在敌人根节点上）。</summary>
        IHealth health;

        /// <summary>子弹配置（只用来取燃烧粒子预制体和高度）。</summary>
        ProjectileConfig config;

        /// <summary>每秒燃烧伤害（施加时的快照）。</summary>
        float damagePerSecond;

        /// <summary>每隔多少秒跳一次伤害（施加时的快照）。</summary>
        float tickInterval;

        /// <summary>距燃烧结束还剩多少秒。</summary>
        float remaining;

        /// <summary>距离下一次跳伤害还有多少秒。</summary>
        float tickTimer;

        /// <summary>伤害来源（传子弹，保持击杀奖励判定和直击一致）。</summary>
        object sender;

        /// <summary>挂在目标身上的燃烧粒子实例（没有配置预制体时为 null）。</summary>
        GameObject vfxInstance;


        /// <summary>
        /// 当前是否正在执行"燃烧跳伤"。
        /// 供 EnemyHealth 判断：燃烧伤害不要触发受击状态（否则会反复打断敌人，甚至卡在受击状态）。
        /// </summary>
        public static bool IsBurnTick { get; private set; }

        /// <summary>
        /// 施加燃烧。重复命中时：刷新持续时间、数值取最新一次施加的（不叠层），粒子不会重复生成。
        /// </summary>
        /// <param name="target">目标血量组件</param>
        /// <param name="context">本次命中的上下文（数值读 Stats，粒子预制体读 Config）</param>
        public static void ApplyTo(IHealth target, in BulletEffectContext context)
        {
            // 目标已经死了 / 配置缺失 / 关闭了燃烧，直接忽略
            if (target == null || !target.IsAlive)
                return;

            if (context.Stats.BurnDamagePerSecond <= 0f || context.Stats.BurnDuration <= 0f)
                return;

            // IHealth 的实现都是 MonoBehaviour，挂在敌人根节点上
            Component component = target as Component;
            if (component == null)
                return;

            // 同一个目标只保留一个燃烧组件
            BurnEffect burn = component.GetComponent<BurnEffect>();
            if (burn == null)
                burn = component.gameObject.AddComponent<BurnEffect>();

            burn.Setup(target, context);
        }

        void Setup(IHealth target, in BulletEffectContext context)
        {
            health = target;
            config = context.Config;
            sender = context.Sender;

            // 把数值拷到自己身上：之后本局数值再变化，不影响已经点燃的目标
            damagePerSecond = context.Stats.BurnDamagePerSecond;
            tickInterval = context.Stats.BurnTickInterval;

            // 重复命中：刷新时长（dps 用最新一次的值，天然"取较大值"的效果由设计决定）
            remaining = Mathf.Max(remaining, context.Stats.BurnDuration);

            // 让第一次跳伤在下一帧立刻发生，命中反馈更直接
            if (tickTimer <= 0f)
                tickTimer = GetTickInterval();

            // 已经有粒子就不重复生成
            EnsureVfx();
        }

        /// <summary>跳伤间隔，做一层保护避免配置成 0 导致每帧掉血。</summary>
        float GetTickInterval()
        {
            return tickInterval > 0f ? tickInterval : 0.5f;
        }

        void Update()
        {
            // 已经烧完：清理粒子并销毁自己
            if (remaining <= 0f)
            {
                Dispose();
                return;
            }

            // 目标死了 / 被销毁：立即结束
            if (health == null || !health.IsAlive)
            {
                remaining = 0f;
                Dispose();
                return;
            }

            float deltaTime = Time.deltaTime;
            remaining -= deltaTime;
            tickTimer += deltaTime;

            float tick = GetTickInterval();

            if (tickTimer >= tick)
            {
                tickTimer -= tick;

                // 每跳至少 1 点伤害，避免 dps * interval 取整后变成 0
                int damage = Mathf.Max(1, Mathf.RoundToInt(damagePerSecond * tick));

                // 走和普通伤害完全相同的入口：血条刷新、死亡、奖励都会正常触发
                // IsBurnTick 只是告诉 EnemyHealth：这一次是燃烧跳伤，不要进受击状态
                // 用 try/finally 保证即使伤害链路里抛异常，标志位也会复位
                IsBurnTick = true;
                try
                {
                    health.TakeDamage(damage, sender, new DamageDirectionEventArgs(damage, Vector3.up));
                }
                finally
                {
                    IsBurnTick = false;
                }
            }
        }

        /// <summary>
        /// 生成燃烧粒子：作为目标的子物体，跟随目标移动。
        /// 粒子系统本身是世界空间模拟，所以敌人走动时火焰会留在身后形成拖尾。
        /// </summary>
        void EnsureVfx()
        {
            if (vfxInstance != null || config == null || config.burnVfxPrefab == null)
                return;

            Component targetComponent = health as Component;
            if (targetComponent == null)
                return;

            vfxInstance = Instantiate(config.burnVfxPrefab, targetComponent.transform);
            vfxInstance.name = "BurnVfx(Runtime)";

            // 挂在躯干高度，避免火焰从脚底冒出来
            vfxInstance.transform.localPosition = new Vector3(0f, config.burnVfxHeight, 0f);
            vfxInstance.transform.localRotation = Quaternion.identity;
        }

        /// <summary>销毁粒子实例（如果有）。</summary>
        void DestroyVfx()
        {
            if (vfxInstance != null)
            {
                Destroy(vfxInstance);
                vfxInstance = null;
            }
        }

        /// <summary>结束燃烧：先清表现，再销毁组件本身。</summary>
        void Dispose()
        {
            DestroyVfx();
            Destroy(this);
        }

        /// <summary>
        /// 目标被池子回收 / 被关闭时清理。
        /// 这一步是必须的：否则敌人复用后还会带着上一局的燃烧状态和粒子。
        /// </summary>
        void OnDisable()
        {
            remaining = 0f;
            tickTimer = 0f;
            Dispose();
        }

        /// <summary>兜底：组件被销毁时确保粒子也一起清掉。</summary>
        void OnDestroy()
        {
            DestroyVfx();
        }
    }
}