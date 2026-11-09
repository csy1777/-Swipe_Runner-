using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class PlayerStats : MonoBehaviour
    {
        private int currentHealth;
        private int currentDamage;
        private int rocketDamage;
        private float currentSpeed;
        private int currentBullets;
        private float fireInterval;

        private ILogService logService;
        private IPlayerContext playerContext;
        private ProjectileConfig projectileConfig;
        private PlayerConfig playerConfig;
        private TurretConfig turretConfig;
        private RocketConfig rocketConfig;

        public float FireInterval => fireInterval;
        public int RocketDamage => rocketDamage;
        public int CurrentHealth => currentHealth;
        public int CurrentDamage => currentDamage;
        public float CurrentSpeed => currentSpeed;
        public int CurrentBullets => currentBullets;
        public int LowBulletsThreshold => Mathf.RoundToInt(turretConfig.lowBulletsThreshold * currentBullets); 

        [Inject]
        private void Construct(IPlayerContext playerContext, 
                               ProjectileConfig projectileConfig,
                               PlayerConfig playerConfig,
                               TurretConfig turretConfig,
                               RocketConfig rocketConfig,
                               ILogService logService)
        {
            this.playerContext = playerContext;
            this.projectileConfig = projectileConfig;
            this.playerConfig = playerConfig;
            this.turretConfig = turretConfig;
            this.rocketConfig = rocketConfig;
            this.logService = logService;
        }

        public void Initialize()
        {
            CalculateAmmoAmount();
            CalculateFireInterval();
            CalculateProjectileDamage();
            CalculateHealth();
            CalculateSpeed();
            CalculateRocketDamage();
        }

        public int CalculateAmmoAmount(int level)
        {
            if (level < 0) return 0;
            return Mathf.RoundToInt(turretConfig.baseBulletAmount + level * turretConfig.bulletAmountMultiplier);
        }
       
        private void CalculateAmmoAmount()
        {
            int level = playerContext.UpgradeLevel.GetLevel(PartType.Ammo);
            currentBullets = CalculateAmmoAmount(level);
            logService.Log("Current bullets " + currentBullets);
        }

        private void CalculateFireInterval()
        {
            int level = playerContext.UpgradeLevel.GetLevel(PartType.Turret);
            fireInterval = (level < 0) ? 0 : turretConfig.baseFireInterval - level * turretConfig.fireIntervalGrowthPerLevel;
            logService.Log("Fire interval " + fireInterval);
        }

        private void CalculateProjectileDamage()
        {
            int level = playerContext.UpgradeLevel.GetLevel(PartType.Turret);
            currentDamage = (level < 0) ? 0 : Mathf.RoundToInt(projectileConfig.baseDamage + level * projectileConfig.damageGrowthPerLevel);
            logService.Log("Turret damage " + currentDamage);
        }


        private void CalculateHealth() // Armor
        {
            int level = playerContext.UpgradeLevel.GetLevel(PartType.Armor);
            currentHealth = Mathf.RoundToInt(playerConfig.baseHealth + (level + 1) * playerConfig.healthGrowthPerLevel);
            logService.Log("Player health " + currentHealth);
        }

        private void CalculateSpeed()
        {
            int level = playerContext.UpgradeLevel.GetLevel(PartType.Wheel);
            currentSpeed = (level < 0) ? 0f : playerConfig.baseMoveSpeed + (level + 1) * playerConfig.speedGrowthPerLevel;
            logService.Log("Player speed " + currentSpeed);
        }

        private void CalculateRocketDamage()
        {
            int level = playerContext.UpgradeLevel.GetLevel(PartType.Rocket);
            rocketDamage = (level < 0) ? 0 : Mathf.RoundToInt(rocketConfig.baseDamage + level * rocketConfig.damageGrowthPerLevel);
            logService.Log("Rocket damage " + rocketDamage);
        }
    }
}