using UnityEngine;

namespace KrolStudio
{
    [CreateAssetMenu(fileName = "TentacleConfig", menuName = "Configs/TentacleConfig")]
    public class TentacleConfig : ScriptableObject
    {
        public float rotationSpeed = 5f;
        public float triggerDistance = 30f;
        public float attackInterval = 1f;
        //public LayerMask damageableTargets; // Objects that can take damage
        public LayerMask detectableTargets;   // Objects that need to be detected

        [Space]
        public float sphereCastRadius;
        public float sphereCastDistance;

        [Space]
        public int baseHealth = 20;
        public float healthGrowthPerLevel = 5;

        [Space]
        public int baseDamage = 5;
    }
}