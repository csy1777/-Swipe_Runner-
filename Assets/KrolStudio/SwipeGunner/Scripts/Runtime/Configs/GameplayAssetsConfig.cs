using UnityEngine;
using UnityEngine.Audio;

namespace KrolStudio
{
    [CreateAssetMenu(fileName = "GameplayAssetsConfig", menuName = "Configs/GameplayAssetsConfig", order = 0)]
    public class GameplayAssetsConfig : ScriptableObject
    {
        [Header("POOL CONTEXT")]
        public DamagePopup damagePopupPrefab;
        public int initialDamagePopupPoolCount = 10;
        [Space]
        public EnemyKillRewardText enemyKillRewardTextPrefab;
        public int initialEnemyKillRewardTextPoolCount = 10;
        [Space]
        public Enemy enemyPrefab;
        public int initialEnemyPoolCount = 40;
        [Space]
        public Tentacle tentaclePrefab;
        public int initialTentaclePoolCount = 20;
        [Space]
        public Projectile projectilePrefab;
        public int initialProjectilePoolCount = 20;

        [Header("SCENE CONTEXT")]
        public LoadingCurtain curtainPrefab;
        public UpgradePartController partPrefab;
        public AudioMixer audioMixer;
    }
}
