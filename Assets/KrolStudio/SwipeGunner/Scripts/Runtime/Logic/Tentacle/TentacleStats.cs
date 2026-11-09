using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class TentacleStats : MonoBehaviour
    {
        int currentHealth;

        TentacleConfig config;
        IHealth health;

        public int CurrentHealth => currentHealth;
        public int CurrentDamage => Mathf.RoundToInt(config.baseDamage * health.Ratio);

        [Inject]
        void Construct(TentacleConfig config)
        {
            this.config = config;
        }

        public void Initialize(int level, IHealth health)
        {
            this.health = health;
            currentHealth = GetEnemyHealth(level);
        }

        int GetEnemyHealth(int level)
        {
            return Mathf.RoundToInt(config.baseHealth + (level - 1) * config.healthGrowthPerLevel);
        }
    }
}