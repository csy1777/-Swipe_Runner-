using Cysharp.Threading.Tasks;
using Zenject;

namespace KrolStudio
{
    public class TentacleDeadState : IState
    {
        readonly ITentacleContext tentacleContext;

        [Inject]
        public TentacleDeadState(ITentacleContext tentacleContext)
        {
            this.tentacleContext = tentacleContext;
        }

        public UniTask Enter()
        {
            tentacleContext.TentacleAnimator.PlayDeath();
            return default;
        }

        public UniTask Exit()
        {
            return default;
        }
    }
}