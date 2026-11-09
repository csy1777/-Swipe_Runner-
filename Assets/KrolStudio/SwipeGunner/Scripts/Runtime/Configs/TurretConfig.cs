using UnityEngine;

namespace KrolStudio
{
    [CreateAssetMenu(fileName = "TurretConfig", menuName = "Configs/TurretConfig")]
    public class TurretConfig : ScriptableObject
    {
        [Header("Rotation Settings")]
        public float rotationSensitivity = 0.1f;
        public float minRotationAngle = -45f;
        public float maxRotationAngle = 45f;
        public bool inverseRotation = false;

        [Header("Firing Settings")]
        public float baseFireInterval = 3f;
        public float fireIntervalGrowthPerLevel = 0.5f;

        [Header("Laser Settings")]
        public float laserMaxDistance = 20f;

        [Header("Bullets")]
        public int baseBulletAmount = 120;
        public float bulletAmountMultiplier = 50f;
        [Range(0f, 1f)] public float lowBulletsThreshold = 0.1f;
    }
}