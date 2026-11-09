using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class TentacleTriggerChecker
    {
        readonly IPlayerContext playerContext;
        readonly ITentacleContext tentacleContext;
        readonly TentacleConfig config;

        [Inject]
        public TentacleTriggerChecker(IPlayerContext playerContext,
                                    ITentacleContext tentacleContext,
                                    TentacleConfig config)
        {
            this.playerContext = playerContext;
            this.tentacleContext = tentacleContext;
            this.config = config;
        }

        public bool CanTriggered()
        {
#if UNITY_EDITOR
            DrawCircle(tentacleContext.Transform.position, config.triggerDistance, tentacleContext.Transform.up, Color.red);
#endif
            return playerContext.PlayerHealth.Current > 0 && Vector3.Distance(tentacleContext.Transform.position, playerContext.Transform.position) <= config.triggerDistance;
        }

        private void DrawCircle(Vector3 center, float radius, Vector3 up, Color color, int segments = 24)
        {
            float angleStep = 360f / segments;
            Quaternion rotation = Quaternion.AngleAxis(angleStep, up);
            Vector3 forward = Vector3.forward * radius;

            Vector3 prevPoint = center + forward;

            for (int i = 0; i <= segments; i++)
            {
                forward = rotation * forward;
                Vector3 nextPoint = center + forward;
                Debug.DrawLine(prevPoint, nextPoint, color);
                prevPoint = nextPoint;
            }
        }
    }
}
