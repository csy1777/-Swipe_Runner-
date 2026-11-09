using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class Bootstrapper : MonoBehaviour
    {
        GameStateMachine gameStateMachine;

        const int TargetFrameRate = 60;

        [Inject]
        void Construct(GameStateMachine gameStateMachine)
        {
            this.gameStateMachine = gameStateMachine;
        }

        void Start()
        {
            Application.targetFrameRate = TargetFrameRate;

            gameStateMachine.Enter<BootstrapState>().Forget();
        }
    }
}