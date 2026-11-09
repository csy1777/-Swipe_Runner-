using System;
using Zenject;
 
namespace KrolStudio
{
    public class UpgradeSystem : IProgressSaver, IProgressReader
    {
        private int incomeLevel = 1;
        private int addPartlevel = 1;
        
        readonly EnemyConfig enemyConfig;

        [Inject]
        public UpgradeSystem(EnemyConfig enemyConfig, ISaveLoadService saveLoadService)
        {
            this.enemyConfig = enemyConfig;

            saveLoadService.Register(SaveKey.UpgradeSystem, this);
        }

        public event Action UpgradeLevelIncreased;
        public event Action IncomeLevelIncreased;

        public int AddPartlevel => addPartlevel;
        public int IncomeLevel => incomeLevel;

        public void IncreaseUpgradeLevel()
        {
            addPartlevel++;
            UpgradeLevelIncreased?.Invoke();
        }

        public void IncreaseIncomeLevel()
        {
            incomeLevel++;
            IncomeLevelIncreased?.Invoke();
        }

        public int GetNextUpgradePartCost()
        {
            return GetValueForLevel(addPartlevel);
        }

        public int GetCurrentIncomeCost()
        {
            return GetValueForLevel(incomeLevel);
        }

        public int GetValueForLevel(int level)
        {
            int value = (5 * level * level + 5 * level + 10) / 2;
            return value;
        }

        // Returns the monetary reward for killing an enemy.
        public float GetEnemyKillReward(bool x2)
        {
            return (enemyConfig.baseReward + enemyConfig.rewardGrowthPerLevel * (incomeLevel - 1)) * (x2 ? 2f : 1f);
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            progress.IncomeLevel = incomeLevel;
            progress.AddPartlevel = addPartlevel;
        }

        public void LoadProgress(PlayerProgress progress)
        {
            incomeLevel = progress.IncomeLevel;
            addPartlevel = progress.AddPartlevel;
        }
    }
}