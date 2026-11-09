# 交接文档 · Swipe Runner（roguelite 改造）

> 用途：换工作区 / 开新会话时，让新的 AI 会话在 1 分钟内恢复全部上下文。
> 用法：把本文件复制到新工程根目录，新会话第一句说「先读 `_HANDOFF.md`，然后继续做阶段 5（卡池 + 商店）」。
> 生成时间：2026-11-09（阶段 1~4 完成后更新）

---

## 一、工程概况

| 项 | 值 |
|---|---|
| Unity | 6000.3.15f1，URP |
| 目录结构 | `Assets/KrolStudio/SwipeGunner/`（游戏）、`Assets/KrolStudio/Plugins/`（Zenject / UniTask / DOTween / GoogleMobileAds） |
| 依赖注入 | Zenject：`Resources/ProjectContext.prefab`（全局容器）+ 每个场景一个 `SceneContext` |
| 异步 | UniTask |
| 入口场景 | `Bootstrap.unity` → 加载 `Level.unity` |
| 关键流程 | `InitGameplayState` → `LaunchState`（零件升级界面）→ `FireModeSelect` → `Tutorial` → `GameLoop` → `Win/Lose` |
| 场景重载 | **每次通关或失败都会 `sceneLoader.Load("Level")` 重载整个场景** |

### 关键认知（踩过的坑，别重复踩）

1. **场景级服务不跨关**：凡是需要跨关保留的状态（如本局 buff），必须放 ProjectContext 容器（`GameInstaller`），不能放 `GameplayInstaller`。
2. **玩家不是场景对象**：`PlayerContext` 由 `GameplayInstaller` 用 `FromComponentInNewPrefab` 懒实例化；敌人也是池化的。
3. **敌人没有刚体**：移动全靠状态机改 `transform.position`；攻击分两套（Chase 是代码直调，Attack 是动画事件）。
4. **UI 走 WindowService**：`WindowBase` + `WindowFactory` + `WindowType`；`Open(type, overlayPage)` 支持叠加，但 `Close()` 有栈行为，别滥用。
5. **数值和资产分离**：会随卡牌变的数值进 `BulletStats`（本局快照），粒子/材质这类资产留在 `ProjectileConfig`。**运行期绝不写 ScriptableObject**（见「已知坑」）。

---

## 二、当前已完成的功能

### 1. 子弹效果流水线（阶段 1）

`Projectile.HandleAttack` 只做两件事：扣直击伤害、把命中交给 `BulletEffects`。具体效果（爆炸/燃烧/冰冻）全在效果列表里。

| 文件（`Logic/Projectile/`） | 说明 |
|---|---|
| `BulletEffects/IBulletEffect.cs` | `void Apply(IHealth target, in BulletEffectContext context)` |
| `BulletEffects/BulletEffects.cs` | 统一入口 `ApplyShot` / `ApplyAll` + 效果注册表；单个效果抛异常只记日志 |
| `BulletEffects/BulletEffectContext.cs` | 只读结构体：`Config / Stats / Sender / HitPoint / HitDirection / DirectTarget / Damage / IsDirectHit` |
| `BulletEffects/ExplosionBulletEffect.cs` | 爆炸：直击额外倍率 + 范围衰减伤害 + 播特效 |
| `BulletEffects/BurnBulletEffect.cs` | 燃烧：转调 `BurnEffect` |
| `BulletEffects/FreezeBulletEffect.cs` | 冰冻：转调 `FreezeEffect` |

新增效果：写一个 `IBulletEffect` → 加进 `BulletEffects.Effects` 数组。调用点不用动。

> 名字别混：`Logic/ExplosionEffect/ExplosionEffect.cs` 是敌人死亡碎成零件的 MonoBehaviour，与子弹无关。

### 2. 范围伤害独立层 + 本局数值（阶段 3）

**范围伤害不再属于"爆炸"，而是独立的一层**：它只决定"一次命中作用于几个敌人"。

| 文件 | 说明 |
|---|---|
| `Logic/Projectile/BulletStats.cs` | 一局的数值快照（结构体）：`AoeRadius / ExplosionDirectExtra / ExplosionDamageMultiplier / ExplosionFalloff / BurnDps / BurnDuration / BurnTickInterval / FreezeDuration` |
| `Logic/Buff/IRunBuffService.cs` + `RunBuffService.cs` | 绑在 ProjectContext 的单例：`Stats`（当前快照）、`Reset()`（失败清零）、`SetAoeRadius / AddAoeRadius`（卡牌用） |
| `Logic/Projectile/EnemyQuery.cs` | `FindInRadius(center, radius, results, seen)`：球形查询 + 按 `IHealth` 去重 + 过滤死亡 |
| `Logic/Projectile/ExplosionVfx.cs` | 只剩"在命中点播一次爆炸粒子"，按 `AoeRadius` 缩放（< 1m 时按 1m 播） |
| `Installers/GameInstaller.cs` | `BindInterfacesAndSelfTo<RunBuffService>()` |
| `States/GameplayStateMachine/LoseGameState.cs` | 进状态时 `runBuffService.Reset()`（失败 → 局内层清零） |

当前数值（`SO_ProjectileConfig`）：范围 2m / 爆炸直击 +100%（合计 2 倍）/ 范围伤害 50% + 边缘衰减到 30% / 燃烧 10 dps × 3s，0.5s 一跳 / 冰冻 1.5s。

把 `aoeRadius` 设 0 = 单体 build：所有效果只作用于直击目标，爆炸变成"直击双倍"。

### 3. 子弹爆炸

`ExplosionBulletEffect` 两种结算：
- **直击目标**（`IsDirectHit`）：额外伤害 = `Damage × ExplosionDirectExtra`（默认 1 → 合计 2 倍），同时播一次爆炸特效；
- **范围波及**：`Damage × ExplosionDamageMultiplier × 距离衰减`，击退方向从命中点指向目标。

### 4. 子弹燃烧（DoT + 粒子）

| 文件 | 说明 |
|---|---|
| `Logic/Projectile/BurnEffect.cs` | 挂在敌人身上按 `BurnTickInterval` 跳伤；结束/死亡/回池时销毁并清理粒子；`IsBurnTick` 静态标志 |
| `EnemyHealth.cs` | 燃烧跳伤**不进** `EnemyHitState`（否则会反复打断，甚至卡死在受击状态） |

数值在施加时拷进组件，之后本局数值变化不影响已经点燃的目标。

### 5. 冰冻（阶段 2）

冻结 = **停状态机推进 + 动画速度归零 + 身体换成冰蓝材质**；解冻后「原地继续」（不重置行为、不回 Idle）。**没有粒子特效**，纯靠材质变色。

| 文件 | 改动 |
|---|---|
| `Logic/Projectile/FreezeEffect.cs` | 计时 / 刷新时长 / 换冰蓝材质 / 池回收解冻；组件**常驻**不自我销毁（避免 Destroy 延迟到帧末的竞态） |
| `Logic/DamageImpact/DamageFlashEffect.cs` | 新增 `SetPersistentMaterial / ClearPersistentMaterial`；优先级：命中闪白 > 持续材质（冰冻）> 原始材质 |
| `Enemy.cs` | `isFrozen` / `IsFrozen` / `SetFrozen(bool)`；`Update` 早退；`OnEnable` 兜底复位 |
| `IEnemyContext`（在 Enemy.cs 里） | 新增 `bool IsFrozen { get; }` |
| `EnemyAnimator.cs` | 新增 `SetSpeed(float)` |
| `EnemyHealth.cs` | HitState 条件加 `&& !enemyContext.IsFrozen` |
| `StaticAssets/Materials/M_EnemyFrozen.mat` | URP Lit、纯冰蓝 `(0.42, 0.80, 1)`、无贴图、`_Smoothness 0.85` + 微弱自发光 |

### 6. 关闭了 UMP 同意弹窗（开发期）

`Assets/.../Configs/SO_AdsConfig.asset`：`IsUMPEnabled: 0`、`DebugMode: 0`、`DebugGeography: 0`。
**上线前必须改回 `IsUMPEnabled: 1` 并关掉 DebugMode**（面向欧洲发行时的合规要求）。

### 7. 芯片（阶段 4）

局内货币，直接挂在 `WalletService` 上（新建 `CurrencyType.Chips = 2`），**存档自动生效**（`WalletsData` 会带着它一起序列化，不需要新 SaveKey）。

| 来源 | 数值 | 位置 |
|---|---|---|
| 普通怪击杀 | +1 | `SO_EnemyConfig.chipsPerKill` → `EnemyDamageImpact.OnDied` |
| 触手（精英）击杀 | +3 | `SO_TentacleConfig.chipsPerKill` → `TentacleDamageImpact.OnDied` |
| 通关底薪 | +40（奖励关 ×2） | `SO_RewardConfig.chipsPerLevelClear / rewardLevelInterval / rewardLevelChipsMultiplier` |

**关卡结算和关卡推进是分开的**：
- `WinGameState.Enter()` → `LevelRewardService.AwardLevelClear()`（用"刚打完的这一关"的编号判奖励关）；
- `CompleteWindow.OnEnable()` → `levelPlacer.IncrementLevelIndex()`。
两者顺序反了就会算错关号。

显示：`UI/HUD/TemporaryText.cs`（临时方案，商店 UI 做好后摘掉）。
已接好：`Prefabs/Windows/LaunchScreen.prefab` 里的 `TemporaryText` 对象（顶层、右上角，`currencyType = Chips`，`valueText` 指向自己的 TMP）。
注意 LaunchScreen 窗口是**缓存的**（`WindowService.createdWindows`，只 SetActive 开关），所以订阅写在 `OnEnable`/`OnDisable` 里，每次打开都会刷新当前值。

---

## 三、已定的设计（策划案已确认，待实现）

### 双层进度模型

```
永久层（存档）：金币 · 零件等级 · 关卡进度 · 射击模式   → 决定“底线”（加算）
局内层（不存卡）：芯片 · 已购卡 · 层数                  → 决定“这局怎么玩”（乘算+机制）
```

- 失败：卡清零、芯片返还 30%、价格计数归零、**关卡进度与零件保留**；
- 存档：**只存芯片 + 购买次数**（两个 int），卡不存；
- 冷启动：芯片在、卡没了 → 商店可当场重建 build。

### 货币

| 货币 | 来源 | 用途 | 失败 |
|---|---|---|---|
| 金币（现有） | 击杀少量 + 通关奖励 | 零件升级（永久） | 保留 |
| 芯片（新增） | 普通怪 +1、触手/精英 +3、通关底薪 +40（奖励关 ×2） | 商店抽卡（200 起，每次 +60）、重抽 | 返还 30% |

单关敌人数量：Level_1 ≈ 97、Level_2~4 ≈ 170，实际击杀 60~80% → 单关芯片约 60~140。

### 卡牌

| 类型 | 内容 | 出现位置 |
|---|---|---|
| 效果卡（全 buff） | 爆裂弹头 / 燃烧弹头 / 冰霜弹头 / 范围伤害 / 连锁爆炸（史诗） | **仅每 5 关奖励关免费 3 选 1**，商店不卖 |
| 数值卡 | 射速、弹量、爆炸倍率、范围半径、燃烧 dps/时长、冰冻时长、穿甲、过热恢复 | 商店（花芯片）+ **每 3 关免费** |
| debuff 卡 | 过载弹匣、不稳定装药、轻量化（高收益带代价） | 随机混入数值卡池，3 张候选最多 1 张，可拒绝 |

### 节奏

```
level % 5 == 0                   → 奖励关 + 效果卡免费抽（必出元素）
level % 3 == 0 && level % 5 != 0 → 数值卡免费抽
level % 15 == 0                  → 双抽（效果卡 + 数值卡）
商店：每关都可打开（在 LaunchScreen 内），价格随购买次数递增
```

### UI 方案（重要）

商店 **不做成状态机状态**，而是 `LaunchScreen` 内的一个面板（`SetActive` 切换），避免窗口栈和场景重载问题。

---

## 四、待办（按顺序）

| 阶段 | 内容 | 状态 |
|---|---|---|
| 0 | git 基线（新工程已建） | ✅ 用户已完成 |
| 1 | **`BulletEffects` 流水线**：爆炸 + 燃烧收进效果列表 | ✅ 已完成 |
| 2 | **冰冻效果** | ✅ 已完成 |
| 3 | **`BulletStats` + `RunBuffService` + 范围伤害独立层** | ✅ 已完成 |
| 4 | **芯片**：击杀 +1/+3、通关 +40（奖励关 ×2）+ `TemporaryText` 显示 | ✅ 已完成 |
| 5 | `BuffCardConfig` + 卡池 SO + `ShopPanel`（LaunchScreen 内）+ 解锁骨架 | ⬜ 下一步 |
| 6 | 3/5 节点免费抽 + 失败重置 | ⬜ |
| 7 | 芯片存档（`PlayerProgress` + `SaveKey`） | ⬜ |

### 一次命中现在是怎么走的

```
Projectile.HandleAttack
├─ Attack(target, damage)                      直击伤害（PlayerStats.CurrentDamage）
└─ BulletEffects.ApplyShot(context)            Stats = runBuffService.Stats
     ├─ 1) 直击目标：永远走一遍（即使已经被直击伤害打死）
     │     ExplosionBulletEffect  → 加播特效 + 额外倍率伤害
     │     BurnBulletEffect       → BurnEffect.ApplyTo
     │     FreezeBulletEffect     → FreezeEffect.ApplyTo
     └─ 2) AoeRadius <= 0 ? 结束
           AoeRadius >  0 ? EnemyQuery.FindInRadius(HitPoint, AoeRadius)
              └─ 每个目标（IsDirectHit = false）：再跑一遍同样的效果列表
```

要点：
- **作用对象**（打一个还是打一片）只在 `ApplyShot` 决定，效果本身不知道"范围"这回事；
- 三个效果谁都不依赖谁（燃烧不再寄生在爆炸的溅射循环里）；
- `ImpactExplosion` 已删除：范围查询 → `EnemyQuery`，伤害 → `ExplosionBulletEffect`，特效 → `ExplosionVfx`。

### 阶段 1/2/3 的实现细节与坑

**效果流水线**
- `ApplyAll` 只过滤空引用，**不过滤死亡目标**：直击那一枪可能已经把目标打死，但爆炸特效/范围伤害仍要触发。所以每个效果都必须能安全处理死亡目标（`TakeDamage` 对 0 血是空操作；燃烧/冰冻自己判 `IsAlive`）。
- 静态数组 `BulletEffects.Effects` 跨场景存活；效果实例必须无状态（有状态的东西挂到目标身上，或放进 `RunBuffService`）。
- 效果数组的顺序 = 施放顺序。

**范围伤害**
- 没有范围技能时**完全不查物理**（比之前每发都查还省）。
- 半径来自 `Stats.AoeRadius`；想调就改 `RunBuffService`（卡牌）或 SO 的 `aoeRadius`（基准值）。

**冰冻**
- **外观只能换材质，不能乘色**：敌人 `M_BlackStickman` 是白底 + 黑色贴图，给 `_BaseColor` 乘冰蓝结果仍是黑。所以走 `DamageFlashEffect.SetPersistentMaterial` 整块替换。
- **换材质必须经由 `DamageFlashEffect`**：它持有渲染器列表和原始材质；自己换的话，命中闪白结束时 `ResetMaterial` 会把冰蓝抹掉。
- **早退条件必须带 `Current > 0`**：`EnemyDeadState` 也靠 `Enemy.Update` 驱动（离开视野回收、玩家靠近推一把），无条件早退会导致尸体永远留在场上。
- **不能用 `SetActive(false)` 冻结**：敌人会消失、`FreezeEffect` 也被禁用 → 没人倒计时 → 永远解不开；也不能用 `enemy.enabled = false`：解冻时触发 `OnEnable`（回池重置逻辑）会回满血、回 Idle。
- **冰冻只支持 Stickman（`Enemy`）**：`Tentacle` 走另一套 `TentacleStateMachine`，`ApplyTo` 拿不到 `Enemy` 会直接返回。

---

## 五、已知坑（务必遵守）

| 坑 | 说明 / 规避 |
|---|---|
| **PhysX 原生崩溃** | `ScKinematicUpdateTask` 空指针，`0xc0000005 @ Unity.dll+0xA44F60`，发生在物理步内。**不要在 FixedUpdate 里创建/销毁刚体、改 isKinematic/velocity**。当前范围伤害是同步结算（和原来 `ImpactExplosion.Resolve` 一样）；如果压测又崩，把 `BulletEffects.ApplyShot` 挪到 `UniTask.NextFrame` 里延迟一帧 |
| **HitState 死锁** | `EnemyHitState.Enter` 只处理“上一个状态是 Idle/Wander/Chase”，其他状态进去不会播动画也没有回调 → 永久卡住。**DoT 类伤害必须跳过 HitState**（`BurnEffect.IsBurnTick`）；**冻结时同理**（`Animator.speed = 0` 时受击动画永远播不完，所以要 `!enemyContext.IsFrozen`） |
| **别在运行期写 ScriptableObject** | `SO_ProjectileConfig` 是全局资产。运行期改它会污染所有对局；而且**编辑器退出 Play 会自动回滚**，导致编辑器里测不出来、打包后才出错。本局数值一律走 `RunBuffService` |
| **场景重载丢状态** | 跨关数据必须放 ProjectContext 级服务（`RunBuffService` 就是） |
| **静态状态跨局残留** | `static` 字段不会随场景重载重置。效果实例保持无状态；跨局数据放服务里并由 `Reset()` 清空 |
| **WindowService 栈** | `Open` 会隐藏当前窗口并压栈，`Close` 会重开栈顶；商店建议做成面板而不是新窗口 |
| **`.meta` 不能丢** | 迁移/复制工程时丢 meta = GUID 变化 = 场景与预制体引用全断 |
| **Level_1 有断引用（历史遗留）** | `Level_1.prefab` 里的 `WallContext` 物体挂着已删除的 `Wall` / `WallHealth` / `WallDamageImpact`（guid 在工程里都找不到），每次 Play 都会刷 `The referenced script on this Behaviour is missing!`。该物体 `m_IsActive: 0`，暂不影响玩法 |
| **仓库体积** | `.gitignore` 必须排除 `Library/ Temp/ Logs/ obj/ Build/ UserSettings/` |

---

## 六、工具与环境

- **Unity MCP**：会话内可用 `mcp__unity__*` 工具（读场景、执行 C#、编译校验）。掉线先 `mcp_connector_reconnect`（key：`json-unity`）；仍失败说明 Unity 没开或 `Window > MCP for Unity` 里的 Server 没启动。
- **编译校验流程**：`refresh_unity(compile=request)` → `read_console(types=["error"])`。
- **pwsh 可能不可用**：沙箱给工作区授予写权限时会报 `SetNamedSecurityInfoW failed (Win32 5)`，此时改用 `read`/`grep`/`glob` 或 MCP 的 `execute_code`。
- **Play 模式自动化测试有限制**：工程 `Run In Background` 是关的，编辑器一失焦 `Time.time` 就停；用 MCP 自动跑 Play 测试时记得先确认焦点，否则子弹根本不飞。
- **备份**：旧工程里有 `_Backup_Explosion_2026-11-06/` 与 `_Backup_Burn_2026-11-06/`。

---

## 七、常用入口文件

| 想改什么 | 去哪 |
|---|---|
| 一次命中打谁（单体/范围） | `Logic/Projectile/BulletEffects/BulletEffects.cs`（`ApplyShot`）、`Logic/Projectile/EnemyQuery.cs` |
| 新增/调整效果 | `Logic/Projectile/BulletEffects/*BulletEffect.cs` + `BulletEffects.Effects` 数组 |
| 本局数值（范围半径、爆炸倍率、燃烧/冰冻） | `Logic/Buff/RunBuffService.cs`、`Logic/Projectile/BulletStats.cs`；基准值在 `SO_ProjectileConfig` |
| 芯片 / 关卡结算 | `Logic/Reward/LevelRewardService.cs`、`SO_RewardConfig`、`SO_EnemyConfig` / `SO_TentacleConfig` 的 `chipsPerKill` |
| 货币显示（临时） | `UI/HUD/TemporaryText.cs`（按货币类型订阅）；`MoneyText.cs` 也按类型过滤了 |
| 爆炸/燃烧/冰冻的具体表现 | `Logic/Projectile/{BurnEffect,FreezeEffect,ExplosionVfx}.cs` |
| 敌人行为 | `Infrastructure/States/EnemyStateMachine/*`、`Logic/Stickman/Enemy.cs` |
| 关卡与刷怪 | `Logic/Level/LevelPlacer.cs`、`Logic/Placers/EnemyPlacer.cs` |
| 玩家数值 | `Logic/Player/PlayerStats.cs`、`UpgradeLevelController.cs` |
| 弹药/开火 | `Logic/Turret/TurretShooting.cs`、`Logic/Ammo/AmmoTracker.cs` |
| 存档 | `Services/SaveLoadService/*`、`Data/PlayerProgress.cs`、`Infrastructure/Enums.cs`（SaveKey） |
| 全局绑定 | `Installers/GameInstaller.cs`（ProjectContext）/ `GameplayInstaller.cs`（SceneContext） |
| 广告 | `Monetization/**`、`Configs/SO_AdsConfig.asset` |