using UnityEngine;

namespace KrolStudio
{
    public class Turret : MonoBehaviour
    {
        [SerializeField] LineRenderer trajectory;
        [SerializeField] TurretPart[] turretParts;
        [SerializeField] LayerMask hitMask;
        [SerializeField] PartLevelController turretLevel;

        int currentPart;

        public int TurretLevel => turretLevel.Level;
        public Transform TrajectoryPoint => turretLevel.Level < 0 ? null : turretParts[turretLevel.Level].trajectoryPoint;
        public FireComponents[] FireComponents => turretLevel.Level < 0 ? null : turretParts[turretLevel.Level].fireComponents;
        public Transform RotatingPart => turretLevel.Level < 0 ? null : turretParts[turretLevel.Level].rotatingPart;
        public int AmmoToSpend => turretLevel.Level < 0 ? 0 : turretParts[turretLevel.Level].ammoToSpend;
        public bool FireSequentially => turretLevel.Level < 0 ? false : turretParts[turretLevel.Level].fireSequentially;

        public void SetActive(bool active)
        {
            trajectory.enabled = active;
            trajectory.positionCount = active ? 2 : 0;
        }

        public void RenderLaser(float maxDistance)
        {
            if (trajectory.positionCount == 0 || TrajectoryPoint == null) return;

            Vector3 start = TrajectoryPoint.position;
            Vector3 direction = TrajectoryPoint.forward;
            Vector3 end = start + direction * maxDistance;

            if (Physics.Raycast(start, direction, out RaycastHit hit, maxDistance, hitMask))
            {
                end = hit.point;
            }

            trajectory.SetPosition(0, start);
            trajectory.SetPosition(1, end);
        }
    }
}
