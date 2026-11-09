using Cysharp.Threading.Tasks;
using Zenject;

namespace KrolStudio
{
    public class FireModeSelectState : IState
    {
        readonly IWindowService windowService;

        [Inject]
        public FireModeSelectState(IWindowService windowService)
        {
            this.windowService = windowService;
        }

        public UniTask Enter()
        {
            windowService.Open(WindowType.FireModeScreen);
            return UniTask.CompletedTask;
        }

        public UniTask Exit()
        {
            return default;
        }
    }
}