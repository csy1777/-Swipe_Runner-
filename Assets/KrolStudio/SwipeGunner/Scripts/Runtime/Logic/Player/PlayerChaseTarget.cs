using UnityEngine;

namespace KrolStudio
{
    public interface IChaseableTarget
    {
        bool IsPossibleToChase();
        Vector3 GetTargetPosition();
        Vector3 GetClosestTargetPoint(Vector3 near);
    }

    public class PlayerChaseTarget : MonoBehaviour, IChaseableTarget
    {
        Collider playerGraphicsCollider;
       
        Health health;
        
        public bool IsOnFinish { get; set; }

        private void Awake()
        {
            health = GetComponent<Health>();
            playerGraphicsCollider = GetComponent<IPlayerContext>().PlayerGraphicsCollider;
        }

        public bool IsPossibleToChase()
        {
            return health.Current > 0 && !IsOnFinish;
        }

        public Vector3 GetTargetPosition()
        {
            return transform.position;
        }

        public Vector3 GetClosestTargetPoint(Vector3 near)
        {
            return playerGraphicsCollider.ClosestPoint(near);
        }
    }
}
