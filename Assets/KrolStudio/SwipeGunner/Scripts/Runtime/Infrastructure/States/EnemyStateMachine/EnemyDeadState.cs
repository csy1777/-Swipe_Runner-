using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class EnemyDeadState : IState, IUpdatableState
    {
        readonly ILogService logService;
        readonly IPlayerContext playerContext;
        readonly IEnemyContext enemyContext;
        readonly EnemyConfig config;
        readonly EnemyPlacer enemyPlacer;
        readonly Camera cam;

        [Inject]
        public EnemyDeadState(ILogService logService, 
                              IPlayerContext playerContext, 
                              IEnemyContext enemyContext,
                              EnemyConfig config,
                              EnemyPlacer enemyPlacer,
                              [Inject(Id = "MainCamera")] Camera cam)
        {
            this.logService = logService;
            this.playerContext = playerContext;
            this.enemyContext = enemyContext;
            this.config = config;
            this.enemyPlacer = enemyPlacer;
            this.cam = cam;
        }

        public UniTask Enter()
        {
            logService.Log($"Entered {nameof(EnemyDeadState)}");
            return default; 
        }

        public UniTask Exit()
        {
            logService.Log($"Exit {nameof(EnemyDeadState)}");
            return default;
        }

        public void Update()
        {
            if(Time.frameCount % 60 == 0 && !IsVisible(enemyContext.Transform.position))
            {
                enemyContext.Transform.gameObject.SetActive(false);
            }

            if (Vector3.Distance(enemyContext.Transform.position, playerContext.Transform.position) < config.pushRadius)
            {
                enemyContext.Push(playerContext.Transform, config.postDeathPushForce);
            }
        }

        private bool IsVisible(Vector3 position) // Checks if the object is within the camera's view
        {
            Vector3 viewportPos = cam.WorldToViewportPoint(position);
            return viewportPos.z > 0f && viewportPos.x >= 0f && viewportPos.x <= 1f && viewportPos.y >= 0f && viewportPos.y <= 1f;
        }
    }
}
