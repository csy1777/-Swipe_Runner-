using UnityEngine;

namespace KrolStudio
{
    [CreateAssetMenu(fileName = "RocketConfig", menuName = "Configs/RocketConfig")]
    public class RocketConfig : ScriptableObject
    {
        public float upDistance = 5f;   // rocket amplitude
        public float upTime = 1f;       // launch time
        public float arcHeight = 5f;    // arc height
        public float flyTime = 2f;      // flight time to the target

        [Space]
        //public int maxExplodeTargets = 30;
        public int baseReload = 3;
        public float baseRadius = 2f;       // check radius

        public int baseDamage = 10;
        public float damageGrowthPerLevel = 5;

        [Space]
        public float detectionCooldown = 1f;
        public float detectableDistance = 30f;

        [Space]
        public LayerMask detectableTargets;   // objects to detect
    }
}