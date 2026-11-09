using Zenject;

namespace KrolStudio
{
    public class TentacleHealth : Health
    {
        EnemyConfig config;
        ITentacleContext tentacleContext;

        [Inject]
        void Construct(EnemyConfig config, ITentacleContext tentacleContext)
        {
            this.config = config;
            this.tentacleContext = tentacleContext;
        }

        public override void Initialize()
        {
            base.Initialize();

            SetMaxHealth(tentacleContext.TentacleStats.CurrentHealth);
            ResetHealth();
        }
    }
}