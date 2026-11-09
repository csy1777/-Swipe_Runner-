
using System;

namespace KrolStudio
{
    public enum SceneName
    {
        Level = 0,
        Bootstrap = 1
    }

    public enum WindowType
    {
        LaunchScreen = 0,
        TutorialScreen = 1,
        SettingsWindow = 2,
        CompleteScreen = 3,
        FailScreen = 4,
        FireModeScreen = 5,
    }

    public enum FireMode
    {
        Single,      // 单发：每次按下打一枪
        Burst,       // 三连发：每次按下连打三枪
        Auto,        // 全自动：按住连射
        Overheat     // 过热模式：射速越来越快，过热后冷却
    }
    public enum SoundType
    {
        Music,
        SFX
    }

    public enum SoundPlayMode
    {
        None = 0,
        Play = 1,           // Use AudioSource.Play()
        PlayOneShot = 2     // Use AudioSource.PlayOneShot()
    }

    public enum CurrencyType
    {
        None = 0,
        Coins = 1,

        /// <summary>
        /// 芯片：局内货币，用来在商店抽卡。
        /// 存在 WalletService 里（自动进存档）；失败时返还 30%（阶段 6 实现）。
        /// </summary>
        Chips = 2,
    }

    public enum Side { Left, Right, Ahead }

    public enum PartType
    {
        None = 255,
        Wheel = 0,
        Turret = 1,
        Armor = 2,
        Ammo = 3,
        Rocket = 4
    }

    public enum ZoneType
    {
        Circle = 0,
        Rect = 1,
        RectAlong = 2
    }

    public enum EnemyType
    {
        Stickman = 0,
        Tentacle = 1
    }

    [Flags]
    public enum SaveKey
    {
        None = 0,
        SettingsWindow = 1 << 0,          // 1
        LevelPlacer = 1 << 1,             // 2
        UpgradeLevelController = 1 << 2,  // 4
        BoardManager = 1 << 3,            // 8
        UpgradeSystem = 1 << 4,           // 16
        WalletService = 1 << 5,           // 32
        Ads = 1 << 6,                     // 64  

        // An interesting experiment, but errors are possible due to carelessness
        All = SettingsWindow
            | LevelPlacer
            | UpgradeLevelController
            | BoardManager
            | UpgradeSystem
            | WalletService
            | Ads
    }
}