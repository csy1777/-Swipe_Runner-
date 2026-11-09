using UnityEngine;

namespace KrolStudio
{
    public class OrbitTowardCamera : MonoBehaviour
    {
        public Transform relativeTo;
        
        Transform targetCamera;
        float orbitRadius;

        void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main.transform;

            if (relativeTo != null)
                orbitRadius = (transform.position - relativeTo.position).magnitude;
        }

        void LateUpdate()
        {
            Vector3 from = (transform.position - relativeTo.position).normalized;
            Vector3 to = (targetCamera.position - relativeTo.position).normalized;

            Quaternion rotation = Quaternion.FromToRotation(from, to);
            transform.position = relativeTo.position + rotation * from * orbitRadius;

            transform.LookAt(targetCamera.position, Vector3.up);
            transform.Rotate(0, 180, 0);
        }
    }
}