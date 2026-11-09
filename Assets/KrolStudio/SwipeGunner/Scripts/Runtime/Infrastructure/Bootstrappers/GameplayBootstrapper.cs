using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class GameplayBootstrapper : MonoBehaviour
    {
        GameplayStateMachine gameplayStateMachine;

        [Inject]
        void Construct(GameplayStateMachine gameplayStateMachine)
        {
            this.gameplayStateMachine = gameplayStateMachine;
        }

        void Start() // Do not use Awake to avoid warning: Failed to remove SceneContext from SceneContextRegistry.
        {
            gameplayStateMachine.Enter<GameplayBootstrapState>().Forget();
        }

        void Update()
        {
            gameplayStateMachine.UpdateState();
        }
    }
}