using Zenject;

namespace KrolStudio
{
    public class PlayerHealth : Health
    {
        private IPlayerContext playerContext;

        [Inject]
        void Construct(IPlayerContext playerContext)
        {
            this.playerContext = playerContext;
        }

        public override void Initialize()
        {
            base.Initialize();

            SetMaxHealth(playerContext.PlayerStats.CurrentHealth);
            ResetHealth();
        }
    }
}