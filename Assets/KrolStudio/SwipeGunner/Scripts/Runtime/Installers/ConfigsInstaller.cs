using UnityEngine;
using Zenject;

namespace KrolStudio
{
    [CreateAssetMenu(fileName = "CoreConfig", menuName = "Configs/CoreConfig", order = 0)]
    public class ConfigsInstaller : ScriptableObjectInstaller
    {
        [SerializeField] GameplayAssetsConfig gameplayAssetsConfig;
        [SerializeField] WindowAssetsConfig windowAssetsConfig;
        [SerializeField] LevelList levelListConfig;
        [SerializeField] ProjectileConfig projectileConfig;
        [SerializeField] TurretConfig turretConfig;
        [SerializeField] EnemyConfig enemyConfig;
        [SerializeField] PlayerConfig playerConfig;
        [SerializeField] RocketConfig rocketConfig;
        [SerializeField] DefaultPlayerProgress defaultPlayerProgress;
        [SerializeField] TentacleConfig tentacleConfig;
        [SerializeField] TutorialConfig tutorialConfig;
        [SerializeField] RewardConfig rewardConfig;
        [SerializeField] PartUpgradeLevelConfig partUpgradeLevelConfig;
        [SerializeField] AdsConfig adsConfig;
        
        [SerializeField] FireModeConfig fireModeConfig;

        public override void InstallBindings()
        {
            Container.BindInstance(gameplayAssetsConfig);
            Container.BindInstance(windowAssetsConfig);
            Container.BindInstance(levelListConfig);
            Container.BindInstance(projectileConfig);
            Container.BindInstance(turretConfig);
            Container.BindInstance(enemyConfig);
            Container.BindInstance(playerConfig);
            Container.BindInstance(rocketConfig);
            Container.BindInstance(defaultPlayerProgress);
            Container.BindInstance(tentacleConfig);
            Container.BindInstance(tutorialConfig);
            Container.BindInstance(rewardConfig);
            Container.BindInstance(partUpgradeLevelConfig);
            Container.BindInstance(adsConfig);
            Container.BindInstance(fireModeConfig);
        }
    }
}