# 燃烧（持续伤害 + 粒子表现）改动：回退说明

日期：2026-11-06
内容：
1. 子弹命中后，让目标在若干秒内持续掉血（DoT）；
2. 燃烧期间在目标身上挂一个火焰粒子（**运行时 Instantiate，结束时销毁**）。

---

## 一、涉及的文件

| # | 文件 | 改动 |
|---|------|------|
| 1 | `Assets/KrolStudio/SwipeGunner/Scripts/Runtime/Logic/Projectile/BurnEffect.cs` | **新增**：燃烧组件（计时 + 每 tick 扣血 + 粒子生成/销毁） |
| 2 | `Assets/KrolStudio/SwipeGunner/StaticAssets/Prefabs/Particles/VFX_EnemyBurn.prefab` | **新增**：敌人燃烧粒子（2 个循环系统） |
| 3 | `Assets/KrolStudio/SwipeGunner/Scripts/Runtime/Configs/ProjectileConfig.cs` | 追加 `[Header("Burn / 燃烧")]` 5 个字段 |
| 4 | `Assets/KrolStudio/SwipeGunner/Scripts/Runtime/Logic/Projectile/Projectile.cs` | 直击目标调用一次 `BurnEffect.ApplyTo(...)` |
| 5 | `Assets/KrolStudio/SwipeGunner/Scripts/Runtime/Logic/Projectile/ImpactExplosion.cs` | 溅射目标调用一次 `BurnEffect.ApplyTo(...)` |
| 6 | `Assets/KrolStudio/SwipeGunner/Scripts/Runtime/Logic/Stickman/EnemyHealth.cs` | 燃烧跳伤不进 `EnemyHitState`（避免卡死） |
| 7 | `Assets/KrolStudio/SwipeGunner/StaticAssets/Configs/SO_ProjectileConfig.asset` | 填了燃烧数值 + 粒子预制体引用 |

所有改动都用 `[燃烧效果]` 注释标记，便于定位。

---

## 二、只想关掉粒子（保留持续伤害）

把 `SO_ProjectileConfig.asset` 里的 **Burn Vfx Prefab 清空**即可：`BurnEffect.EnsureVfx()` 会因为 `burnVfxPrefab == null` 直接跳过，伤害逻辑完全不受影响。
或者直接把 `VFX_EnemyBurn.prefab` 删掉（记得先清空引用）。

## 三、彻底关掉燃烧

把 **Burn Duration 设为 0**（或 Burn Damage Per Second 设为 0）——`ApplyTo` 会在入口直接返回，不挂组件、不动画。

## 四、完全回退（按顺序）

1. 删除 `BurnEffect.cs` 和 `VFX_EnemyBurn.prefab`（连同各自的 `.meta`）。
2. `ProjectileConfig.cs`：删除 `// ==================== [燃烧效果] 开始` 到 `[燃烧效果] 结束` 之间的整段。
3. `ImpactExplosion.cs`：删除 `// ==== [燃烧效果]` 到 `// ==== [燃烧效果] end ====` 之间的 4 行。
4. `Projectile.cs`：删除 `// ==== [燃烧效果]` 到 `// ==== [燃烧效果] end ====` 之间的 4 行。
5. `EnemyHealth.cs`：把 `TakeDamage` 恢复为：

```csharp
public override void TakeDamage(int damage, object sender, EventArgs e)
{
    base.TakeDamage(damage, sender, e);

    if (Current > 0)
        stateMachine.Enter<EnemyHitState>().Forget();
}
```

6. 回到 Unity 重新编译，Console 无红色报错即可。

---

## 五、当前参数

| 字段 | 值 | 说明 |
|---|---|---|
| burnDamagePerSecond | 10 | 每秒伤害 |
| burnDuration | 3 | 持续 3 秒 |
| burnTickInterval | 0.5 | 每 0.5 秒跳一次 → 每跳 5 点，共 6 跳 ≈ 30 点 |
| burnVfxPrefab | VFX_EnemyBurn | 燃烧粒子 |
| burnVfxHeight | 0.8 | 粒子挂在躯干高度（米） |

## 六、粒子实现方式（性能相关）

当前采用「**每次点燃 Instantiate，结束销毁**」：

- 优点：改动小、不占敌人 prefab、不会残留；
- 代价：每只敌人每次燃烧各一次 `Instantiate` + `Destroy`（含 GC）。

如果以后同屏燃烧数量很多（>10）并且出现帧尖峰，可以改成「**预置在敌人 prefab 上 + SetActive 开关**」：把 `VFX_EnemyBurn` 作为 `EnemyStickmanContext.prefab` 的子物体（默认关闭），`BurnEffect` 里把 `EnsureVfx/DestroyVfx` 换成 `vfx.SetActive(true/false)`，并在 `Enemy.OnEnable` 里强制关闭一次即可。