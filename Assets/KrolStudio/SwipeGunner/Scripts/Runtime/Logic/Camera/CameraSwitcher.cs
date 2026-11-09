using Unity.Cinemachine;
using UnityEngine;

namespace KrolStudio
{
    public class CameraSwitcher : MonoBehaviour
    {
        [SerializeField] private CinemachineBrain cinemachineBrain;

        [SerializeField] private float followBlendTime = 0.4f;
        [SerializeField] private CinemachineBlendDefinition.Styles followBlendStyle;

        [SerializeField] private float finishBlendTime = 1f;
        [SerializeField] private CinemachineBlendDefinition.Styles finishBlendStyle;

        [Space]
        [SerializeField] private CinemachineCamera startCamera;
        [SerializeField] private CinemachineCamera followCamera;
        [SerializeField] private CinemachineCamera finishCamera;

        public Transform FollowCamera => followCamera.transform;

        public void Initialize(Transform target)
        {
            InitializeStartCamera(target);
            InitializeFollowCamera(target);
            InitializeFinishCamera(target);
        }

        private void InitializeFinishCamera(Transform target)
        {
            finishCamera.Follow = target;
            finishCamera.LookAt = target;

            /*
            finishCamera.transform.rotation = ...;
            var transposer = finishCamera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer != null)
                transposer.m_FollowOffset = ...;
            */
        }

        private void InitializeStartCamera(Transform target)
        {
            startCamera.Follow = target;
            startCamera.LookAt = target;

            /*
            startCamera.transform.rotation = ...;
            var transposer = startCamera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer != null)
                transposer.m_FollowOffset = ...;
            */
        }

        private void InitializeFollowCamera(Transform target)
        {
            followCamera.Follow = target;
            followCamera.LookAt = target;

            /*
            followCamera.transform.rotation = ...;
            var transposer = followCamera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer != null)
                transposer.m_FollowOffset = ...;
            */
        }

        public void SwitchToStart()
        {
            startCamera.Priority = 10;
            followCamera.Priority = 0;
            finishCamera.Priority = 0;

            CinemachineBlendDefinition blendDef = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseIn, followBlendTime);
            cinemachineBrain.DefaultBlend = blendDef;
        }

        public void SwitchToFollow()
        {
            startCamera.Priority = 0;
            followCamera.Priority = 10;
            finishCamera.Priority = 0;
        }

        public void SwitchToFinish()
        {
            startCamera.Priority = 0;
            followCamera.Priority = 0;
            finishCamera.Priority = 10;

            CinemachineBlendDefinition blendDef = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseIn, finishBlendTime);
            cinemachineBrain.DefaultBlend = blendDef;
        }
    }
}