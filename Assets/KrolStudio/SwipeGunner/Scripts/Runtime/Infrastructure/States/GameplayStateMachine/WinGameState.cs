using Cysharp.Threading.Tasks;
using Zenject;

namespace KrolStudio
{
    public class WinGameState : IState
    {
        readonly IWindowService windowService;

        [Inject]
        public WinGameState(IWindowService windowService, ISaveLoadService saveLoadService)
        {
            this.windowService = windowService;
        }

        public UniTask Enter()
        {
            windowService.Open(WindowType.CompleteScreen);
            return default;
        }

        public UniTask Exit()
        {
            return default;
        }
    }
}