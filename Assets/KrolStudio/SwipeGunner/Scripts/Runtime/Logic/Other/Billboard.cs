using UnityEngine;

namespace KrolStudio
{
    public class Billboard : MonoBehaviour
    {
        Transform target;

        void Awake()
        {
            if (target == null)
                target = Camera.main.transform;
        }

        public void SetTarget(Transform target)
        {
            this.target = target;
        }

        void LateUpdate() // RotateTowardsCamera
        {
            if (target == null) return;

            transform.LookAt(transform.position + target.forward, target.up);
            //transform.LookAt(transform.position + target.rotation * Vector3.forward, target.rotation * Vector3.up);
        }
    }
}