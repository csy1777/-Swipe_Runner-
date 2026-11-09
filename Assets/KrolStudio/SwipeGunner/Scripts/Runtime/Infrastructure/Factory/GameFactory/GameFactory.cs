using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class GameFactory : IGameFactory
    {
        private IInstantiator instantiator;

        private GameplayAssetsConfig gameplayAssetsConfig;

        [Inject]
        public GameFactory(IInstantiator instantiator, GameplayAssetsConfig gameplayAssetsConfig)
        {
            this.instantiator = instantiator;
            this.gameplayAssetsConfig = gameplayAssetsConfig;
        }

        public Projectile CreateProjectile()
        {
            GameObject newObject = instantiator.InstantiatePrefab(gameplayAssetsConfig.projectilePrefab);
            return newObject.GetComponent<Projectile>();
        }

        public Enemy CreateEnemy()
        {
            GameObject newObject = instantiator.InstantiatePrefab(gameplayAssetsConfig.enemyPrefab);
            return newObject.GetComponent<Enemy>();
        }

        public DamagePopup CreateDamagePopup()
        {
            return Object.Instantiate(gameplayAssetsConfig.damagePopupPrefab);
        }

        public EnemyKillRewardText CreateEnemyKillRewardText()
        {
            return Object.Instantiate(gameplayAssetsConfig.enemyKillRewardTextPrefab);
        }

        public UpgradePartController CreatePart()
        {
            GameObject newObject = instantiator.InstantiatePrefab(gameplayAssetsConfig.partPrefab);
            return newObject.GetComponent<UpgradePartController>();
        }

        public Tentacle CreateTentacle()
        {
            GameObject newObject = instantiator.InstantiatePrefab(gameplayAssetsConfig.tentaclePrefab);
            return newObject.GetComponent<Tentacle>();
        }
    }
}