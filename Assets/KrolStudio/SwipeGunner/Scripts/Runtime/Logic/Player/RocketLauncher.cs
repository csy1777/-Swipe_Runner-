using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class RocketLauncher : MonoBehaviour
    {
        [SerializeField] List<Rocket> rockets;

        private List<ITargetFocus> targets;
        private IRocketHud rocketHud;
        private IPlayerContext playerContext;
        private EnemyPlacer enemyPlacer;
        private RocketConfig rocketConfig;
        private Camera cam;

        private float timer;
        private Collider[] results;

        private readonly List<ITargetFocus> targetsLocal = new();
        private readonly List<Transform> transforms = new();

        [Inject]
        private void Construct(IRocketHud rocketHud, 
                               IPlayerContext playerContext, 
                               EnemyPlacer enemyPlacer,
                               RocketConfig rocketConfig,
                               [Inject(Id = "MainCamera")] Camera cam)
        {
            this.rocketHud = rocketHud;
            this.playerContext = playerContext;
            this.enemyPlacer = enemyPlacer;
            this.rocketConfig = rocketConfig;
            this.cam = cam;

            Initialize(rocketConfig);
        }

        private void Initialize(RocketConfig rocketConfig)
        {
            foreach (var rocket in rockets)
            {
                rocket.Initialize(rocketConfig, playerContext);
            }

            results = new Collider[20]; //[rockets.Count];
        }

        private void OnEnable()
        {
            rocketHud.SubscribeToRocketLaunch(Shoot);
        }

        private void OnDisable()
        {
            rocketHud.UnsubscribeFromRocketLaunch(Shoot);
        }

        public void Tick()
        {
            timer += Time.deltaTime;
            if (timer < rocketConfig.detectionCooldown)
                return;

            timer = 0f;

            if (rocketHud.IsReloading())
            {
                DisableFocus();
            }
            else
            {
                FindTarget();
            }
        }

       
        public void FindTarget()
        {
            int count = Physics.OverlapSphereNonAlloc(
               playerContext.Transform.position,
               rocketConfig.detectableDistance,
               results,
               rocketConfig.detectableTargets,
               QueryTriggerInteraction.Collide
            );

            if (count > 0)
            {
                DisableFocus();

                targetsLocal.Clear();
                transforms.Clear();

                foreach (var c in results)
                {
                    if (c != null && c.transform != null)
                        transforms.Add(c.transform);
                }

                transforms.Sort((a, b) =>
                    ((b.position - playerContext.Transform.position).sqrMagnitude)
                    .CompareTo((a.position - playerContext.Transform.position).sqrMagnitude));

                for (int i = 0; i < count; i++)
                {
                    if(transforms[i].TryGetComponent(out Enemy enemy))
                    {
                        if (!enemy.IsAlive || !IsVisible(enemy.transform.position)) continue;
                        enemy.DisplayFocus(true);
                        targetsLocal.Add(enemy);
                    }
                    else if(transforms[i].TryGetComponent(out Tentacle tentacle))
                    {
                        if (!tentacle.IsAlive || !IsVisible(tentacle.transform.position)) continue;
                        tentacle.DisplayFocus(true);
                        targetsLocal.Add(tentacle);
                    }

                    if (targetsLocal.Count == rockets.Count)
                    {
                        targets = targetsLocal;
                        return;
                    }
                }

                targets = targetsLocal;
                return;
            }

            targets = null;
        }

        private bool IsVisible(Vector3 position) // Checks if the object is within the camera's view
        {
            Vector3 viewportPos = cam.WorldToViewportPoint(position);
            return viewportPos.z > 0f && viewportPos.x >= 0f && viewportPos.x <= 1f && viewportPos.y >= 0f && viewportPos.y <= 1f;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (rocketConfig == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerContext.Transform.position, rocketConfig.detectableDistance);
        }
#endif

        public void DisableFocus()
        {
            if (targets == null) return;

            foreach (ITargetFocus target in targets)
                target.DisplayFocus(false);
        }

        public bool Shoot()
        {
            if (playerContext.PlayerHealth.Current <= 0) return false;

            if (targets == null || targets.Count == 0)
            {
                rocketHud.ShowRocketLaunchFailureMessage();
                return false;
            }

            LaunchAsync().Forget();

            return true;
        }

        public async UniTask LaunchAsync()
        {
            int index = 0;

            try
            {
                foreach (var rocket in rockets)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(0.1f));

                    targets[index].DisplayFocus(false);
                
                    rocket.Launch(targets[index].Transform);

                    index = (index + 1) % targets.Count;
                }
            }         
            catch (MissingReferenceException ex)
            {
                Debug.LogWarning($"The object was destroyed during animation: {ex.Message}", this);
            }
            catch (NullReferenceException ex)
            {
                Debug.LogWarning($"Missing link during animation: {ex.Message}", this);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error: {ex}", this);
            }
        }
    }
}