# 交接文档 · Swipe Runner（roguelite 改造）

> 用途：换工作区 / 开新会话时，让新的 AI 会话在 1 分钟内恢复全部上下文。
> 用法：把本文件复制到新工程根目录，新会话第一句说「先读 `_HANDOFF.md`，然后继续做 BulletEffects 重构」。
> 生成时间：2026-11-09

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

---

## 二、当前已完成的功能（本次会话做的）

### 1. 子弹爆炸（范围伤害）

| 文件 | 说明 |
|---|---|
| `Assets/KrolStudio/SwipeGunner/Scripts/Runtime/Logic/Projectile/ImpactExplosion.cs` | 以命中点为球心 `OverlapSphereNonAlloc`，按距离衰减扣血，播特效；`Resolve`（同步）/ `ResolveDeferred`（延迟一帧） |
| `Projectile.cs` | `TryAttack` 把 `hit.point` 传入 `HandleAttack`；直击伤害后调用爆炸；`OnDrawGizmosSelected` 画爆炸半径 |
| `ProjectileConfig.cs` | 新增 `explosionRadius / explosionDamageMultiplier / explosionFalloff / explosionAffectsDirectTarget / explosionPrefab / explosionVfxLifetime` |
| `Assets/.../Particles/VFX_ExplosionSphere.prefab` | 球形爆炸粒子（3 个系统：外壳扩散 / 中心闪光 / 半径边界），代码里按 `explosionRadius/2` 自动缩放 |

### 2. 子弹燃烧（DoT + 粒子）

| 文件 | 说明 |
|---|---|
| `.../Logic/Projectile/BurnEffect.cs` | 挂在敌人身上，按 `burnTickInterval` 跳伤；结束/死亡/回池时销毁并清理粒子；`IsBurnTick` 静态标志 |
| `EnemyHealth.cs` | 燃烧跳伤**不进** `EnemyHitState`（否则会反复打断，甚至卡死在受击状态） |
| `ProjectileConfig.cs` | 新增 `burnDamagePerSecond / burnDuration / burnTickInterval / burnVfxPrefab / burnVfxHeight` |
| `Assets/.../Particles/VFX_EnemyBurn.prefab` | 敌人燃烧粒子（火焰 + 火星，循环、世界空间） |

当前数值：爆炸半径 2m / 伤害 50%；燃烧 10 dps × 3s / 0.5s 一跳。

### 3. 关闭了 UMP 同意弹窗（开发期）

`Assets/.../Configs/SO_AdsConfig.asset`：`IsUMPEnabled: 0`、`DebugMode: 0`、`DebugGeography: 0`。
**上线前必须改回 `IsUMPEnabled: 1` 并关掉 DebugMode**（面向欧洲发行时的合规要求）。

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
| 效果卡（全 buff） | 爆裂弹头 / 燃烧弹头 / 冰霜弹头 / 连锁爆炸（史诗） | **仅每 5 关奖励关免费 3 选 1**，商店不卖 |
| 数值卡 | 射速、弹量、爆炸伤害/半径、燃烧 dps/时长、穿甲、过热恢复 | 商店（花芯片）+ **每 3 关免费** |
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
| 1 | **`BulletEffects` 重构**：`IBulletEffect` + `BulletEffects` + `BurnEffectApplier`，把 `Projectile` / `ImpactExplosion` 里写死的 `BurnEffect.ApplyTo` 换成一个通用入口（行为不变） | ⬜ 下一步 |
| 2 | **冰冻效果**：`FreezeEffect` + `Enemy.Update` 冻结判断 + `Animator.speed = 0` + `EnemyHealth` 跳过 HitState | ⬜ |
| 3 | `BulletStats` + `RunBuffService`（ProjectContext）→ `Projectile`/`ImpactExplosion` 改读服务 | ⬜ |
| 4 | 芯片（击杀加钱）+ 临时显示 | ⬜ |
| 5 | `BuffCardConfig` + 卡池 SO + `ShopPanel`（LaunchScreen 内） | ⬜ |
| 6 | 3/5 节点免费抽 + 失败重置 | ⬜ |
| 7 | 芯片存档（`PlayerProgress` + `SaveKey`） | ⬜ |

### 阶段 1 的具体设计（下一步要做的）

```csharp
public interface IBulletEffect
{
    void Apply(IHealth target, ProjectileConfig config, object sender);
}

public static class BulletEffects
{
    static readonly List<IBulletEffect> effects = new() { new BurnEffectApplier() };
    public static void ApplyAll(IHealth target, ProjectileConfig config, object sender) { ... }
}
```

调用点（保持两处，语义是“直击效果 / 溅射效果”）：

```csharp
// Projectile.HandleAttack
BulletEffects.ApplyAll(healthHandler, config, this);

// ImpactExplosion.Apply 循环里
BulletEffects.ApplyAll(health, config, sender);
```

---

## 五、已知坑（务必遵守）

| 坑 | 说明 / 规避 |
|---|---|
| **PhysX 原生崩溃** | `ScKinematicUpdateTask` 空指针，`0xc0000005 @ Unity.dll+0xA44F60`，发生在物理步内。**不要在 FixedUpdate 里创建/销毁刚体、改 isKinematic/velocity**；爆炸/死亡表现要延迟一帧或池化。代码里 `ImpactExplosion.ResolveDeferred` 就是为此准备的 |
| **HitState 死锁** | `EnemyHitState.Enter` 只处理“上一个状态是 Idle/Wander/Chase”，其他状态进去不会播动画也没有回调 → 永久卡住。**DoT 类伤害必须跳过 HitState**（现有 `BurnEffect.IsBurnTick` 就是干这个的） |
| **场景重载丢状态** | 跨关数据必须放 ProjectContext 级服务 |
| **WindowService 栈** | `Open` 会隐藏当前窗口并压栈，`Close` 会重开栈顶；商店建议做成面板而不是新窗口 |
| **`.meta` 不能丢** | 迁移/复制工程时丢 meta = GUID 变化 = 场景与预制体引用全断 |
| **仓库体积** | `.gitignore` 必须排除 `Library/ Temp/ Logs/ obj/ Build/ UserSettings/` |

---

## 六、工具与环境

- **Unity MCP**：会话内可用 `mcp__unity__*` 工具（读场景、执行 C#、编译校验）。掉线时用 `mcp_connector_reconnect`（连接 key：`json-unity`）重连；仍失败说明 Unity 没开或 `Window > MCP for Unity` 里的 Server 没启动。
- **编译校验流程**：`refresh_unity(compile=request)` → `read_console(types=["error"])`。
- **pwsh 可能不可用**：沙箱在给工作区授予写权限时会报 `SetNamedSecurityInfoW failed (Win32 5)`，此时改用 `read`/`grep`/`glob` 工具或 MCP 的 `execute_code`。
- **备份**：旧工程里有 `_Backup_Explosion_2026-11-06/`（爆炸原始代码）与 `_Backup_Burn_2026-11-06/`（燃烧回退说明）。

---

## 七、常用入口文件

| 想改什么 | 去哪 |
|---|---|
| 子弹命中 / 伤害 / 爆炸 / 燃烧 | `Logic/Projectile/Projectile.cs`、`ImpactExplosion.cs`、`BurnEffect.cs` |
| 敌人行为 | `Infrastructure/States/EnemyStateMachine/*`、`Logic/Stickman/Enemy.cs` |
| 关卡与刷怪 | `Logic/Level/LevelPlacer.cs`、`Logic/Placers/EnemyPlacer.cs` |
| 玩家数值 | `Logic/Player/PlayerStats.cs`、`UpgradeLevelController.cs` |
| 弹药/开火 | `Logic/Turret/TurretShooting.cs`、`Logic/Ammo/AmmoTracker.cs` |
| 存档 | `Services/SaveLoadService/*`、`Data/PlayerProgress.cs`、`Infrastructure/Enums.cs`（SaveKey） |
| 全局绑定 | `Installers/GameInstaller.cs`（ProjectContext）/ `GameplayInstaller.cs`（SceneContext） |
| 广告 | `Monetization/**`、`Configs/SO_AdsConfig.asset` |