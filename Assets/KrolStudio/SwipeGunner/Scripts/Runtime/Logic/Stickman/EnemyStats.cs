using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class EnemyStats : MonoBehaviour
    {
        int currentHealth;

        EnemyConfig enemyConfig;
        IHealth health;

        public int CurrentHealth => currentHealth;
        public int CurrentDamage => Mathf.RoundToInt(enemyConfig.baseDamage * health.Ratio);

        [Inject]
        void Construct(EnemyConfig enemyConfig)
        {
            this.enemyConfig = enemyConfig;
        }

        public void Initialize(int level, IHealth health)
        {
            this.health = health;
            currentHealth = GetEnemyHealth(level);
        }

        int GetEnemyHealth(int level)
        {
            return Mathf.RoundToInt(enemyConfig.baseHealth + (level - 1) * enemyConfig.healthGrowthPerLevel);
        }
    }
}