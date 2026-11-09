# 爆炸效果改动：备份与回退说明

本目录**不在 Assets 下**，Unity 不会导入/编译它，只是纯粹的备份。
改动日期：2026-11-06

## 本次一共动了 3 个地方

| # | 文件 | 类型 |
|---|------|------|
| 1 | `Assets/KrolStudio/SwipeGunner/Scripts/Runtime/Logic/Projectile/ImpactExplosion.cs` | **新增文件** |
| 2 | `Assets/KrolStudio/SwipeGunner/Scripts/Runtime/Configs/ProjectileConfig.cs` | 末尾追加了一组 `[Header("Explosion")]` 字段 |
| 3 | `Assets/KrolStudio/SwipeGunner/Scripts/Runtime/Logic/Projectile/Projectile.cs` | 改了 `TryAttack()`、`HandleAttack()`，并在 `OnDrawGizmosSelected()` 里加了一段范围可视化 |

## 回退方法（三种，从省事到彻底）

### 方法 A：完全回退（推荐，1 分钟）

1. 删除新增文件：
   `Assets/KrolStudio/SwipeGunner/Scripts/Runtime/Logic/Projectile/ImpactExplosion.cs`
   （如果 Unity 里还生成了同名 `.meta`，一起删掉）
2. 用本目录的 `ProjectileConfig.cs.orig` **覆盖** `Assets/.../Configs/ProjectileConfig.cs`
3. 打开 `Projectile.cs`，把 `TryAttack()` 和 `HandleAttack()` 两个方法，
   用本目录 `Projectile.original-methods.txt` 里的原始版本**整段替换**；
   再把 `OnDrawGizmosSelected()` 里新增的那段 `Gizmo` 爆炸球删掉（有中文注释，很好找）
4. 回到 Unity，等它重新编译，Console 无红色报错即可

### 方法 B：只关掉爆炸，保留文件

- 把 `SO_ProjectileConfig.asset` 里的 `Explosion Radius` 改成 `0`，
  或者把 `Projectile.HandleAttack()` 里调用 `ImpactExplosion.Resolve(...)` 的那一行注释掉。
- 伤害逻辑会恢复成“只打直击目标”。

### 方法 C：如果出现编译错误，先看这几个常见点

- `ImpactExplosion` 找不到：检查文件是否在 `Assets` 目录下、类名与文件名是否一致（`ImpactExplosion.cs` ↔ `class ImpactExplosion`）
- `config.explosionRadius` 等字段报红：说明 `ProjectileConfig.cs` 的修改没保存，或 Unity 还没重新导入 → 右键 `ProjectileConfig.cs` → `Reimport`
- 中文注释乱码：文件保存为 **UTF-8**（Unity 也接受 UTF-8 BOM）

## 原始文件内容（内联备份，防止上面文件丢失）

`ProjectileConfig.cs` 的原始完整内容见 `ProjectileConfig.cs.orig`。
`Projectile.cs` 中两个方法的原始内容见 `Projectile.original-methods.txt`。