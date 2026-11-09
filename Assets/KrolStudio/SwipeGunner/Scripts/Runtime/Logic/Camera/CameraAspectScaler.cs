using Unity.Cinemachine;
using UnityEngine;

namespace KrolStudio
{
    public class CameraAspectScaler : MonoBehaviour
    {
        [Header("Reference")]
        public Vector2 ReferenceResolution = new Vector2(1080, 1920);
        public float referenceFOV = 47.5f; // vertical FOV at reference aspect ratio

        private Camera cam;

        [SerializeField] private CinemachineCamera virtualCamera;

        void Awake()
        {
            cam = Camera.main;
            ApplyAspectRatioFOV();
        }

        void ApplyAspectRatioFOV()
        {
            if (ReferenceResolution.x == 0 || ReferenceResolution.y == 0) return;

            float refAspect = ReferenceResolution.x / ReferenceResolution.y;
            float currentAspect = (float)Screen.width / Screen.height;

            // Convert vertical FOV to radians
            float refFOVrad = referenceFOV * Mathf.Deg2Rad;

            // Calculate horizontal FOV at reference aspect ratio
            float horizontalFOVrad = 2f * Mathf.Atan(Mathf.Tan(refFOVrad / 2f) * refAspect);

            // Compute the vertical FOV needed for current aspect to keep horizontal FOV the same
            float newFOVrad = 2f * Mathf.Atan(Mathf.Tan(horizontalFOVrad / 2f) / currentAspect);

            virtualCamera.Lens.FieldOfView = newFOVrad * Mathf.Rad2Deg;
        }

        // Update when changing screen size / editor values
        //void OnValidate() => ApplyAspectRatioFOV();

        void Update()
        {
            // if resolution can change during runtime (e.g., device rotation)
            ApplyAspectRatioFOV();
        }
    }
}