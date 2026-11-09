using UnityEngine;

namespace KrolStudio
{
    [RequireComponent(typeof(RectTransform))]
    public class RectBillboard : MonoBehaviour
    {
        private Transform target;
        private RectTransform rectTransform;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if(target == null)
                target = Camera.main.transform;
        }

        void LateUpdate()  // RotateTowardsCamera
        {
            if (target == null) return;

            // Rotate the object to face the camera direction
            Vector3 dir = rectTransform.position - target.transform.position;
            rectTransform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
    }
}