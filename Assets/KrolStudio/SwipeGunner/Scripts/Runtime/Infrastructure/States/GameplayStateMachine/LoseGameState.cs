using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class LoseGameState : IState
    {
        readonly IWindowService windowService;

        [Inject]
        public LoseGameState(IWindowService windowService)
        {
            this.windowService = windowService;
        }

        public UniTask Enter()
        {
            windowService.Open(WindowType.FailScreen);
            return default;
        }

        public UniTask Exit()
        {
            return default;
        }
    }
}