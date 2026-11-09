using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using Zenject;

namespace KrolStudio
{
    public class SplineMover : MonoBehaviour
    {
        [TagSelector] [SerializeField] string speedChangerTag;

        [HideInInspector] public SplineContainer spline;

        PlayerConfig config;
        CameraSwitcher cameraSwitcher;
        IPlayerContext playerContext;
        ILogService logService;

        bool isSpeedChanged = false;
        float targetSpeed;
        float splineLength;
        float speed;

        float t = 0f;

        public event System.Action Completed;
        
        public float NormalizedTime => t;

        [Inject]
        void Construct(PlayerConfig config, 
                       CameraSwitcher cameraSwitcher, 
                       IPlayerContext playerContext,
                       ILogService logService)
        {
            this.config = config;
            this.cameraSwitcher = cameraSwitcher;
            this.playerContext = playerContext;
            this.logService = logService;
        }

        public void SetPosition()
        {
            float3 pos, tangent, up;
            SplineUtility.Evaluate(spline.Spline, t, out pos, out tangent, out up);

            transform.position = spline.transform.TransformPoint(pos);
            Quaternion rot = Quaternion.LookRotation(
                spline.transform.TransformDirection(tangent),
                spline.transform.TransformDirection(up)
            );
            transform.rotation = rot;
        }

        public void UpdatePosition()
        {
            speed = Mathf.MoveTowards(speed, targetSpeed, config.acceleration * Time.deltaTime);

            // How much distance to cover along the normalized length this frame
            float distanceDelta = speed * Time.deltaTime;
            float tDelta = distanceDelta / spline.CalculateLength();

            t += tDelta;

            // Clamp or loop:
            t = Mathf.Clamp01(t);

            if (Mathf.Approximately(t, 1f))
            {
                Completed?.Invoke();
                return;
            }

            SetPosition();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (playerContext.PlayerHealth.Current <= 0) return;

            if (other.CompareTag(speedChangerTag))
            {
                isSpeedChanged = !isSpeedChanged;

                if(isSpeedChanged)
                {
                    targetSpeed = playerContext.PlayerStats.CurrentSpeed;
                }
                else
                {
                    targetSpeed = config.startSpeed;
                    playerContext.PlayerHealthBar.Hide();
                    cameraSwitcher.SwitchToFinish();
                }
            }
        }

        public void Restart()
        {
            t = 0f;
        }

        public void SetStartSpeed()
        {
            targetSpeed = config.startSpeed;
        }
    }
}