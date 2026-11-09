using UnityEngine;

namespace KrolStudio
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Configs/EnemyConfig")]
    public class EnemyConfig : ScriptableObject
    {
        public int baseHealth = 10;
        public float healthGrowthPerLevel = 5;

        [Space]
        public int baseDamage = 5;

        [Space]
        public int baseReward = 10;
        public float rewardGrowthPerLevel = 0.5f;

        [Space]
        public float speed = 3.5f;
        public float rotationSpeed = 3f;
        public float walkingSpeed = 2f;

        [Space]
        public float attackDistance = 0.5f;
        public float triggerDistance = 30;
        public float chaseStopDistance = 3;
        public float wanderArrivalDistance = 0.1f;
        public float behindAttackDistance = 1f;
        public float backAngleThreshold = 0.1f;

        [Space]
        public float walkingThreshold = 1f;   // check BlendThree 
        public float runningThreshold = 2f;   // check BlendThree 
        public float accelerationMultiplier = 3f;

        [Space]
        public float pushRadius = 1f;
        public float postDeathPushForce = 5;
        public float deathPushForce = 25f; 

        [Space]
        public Vector2 idleDuration = new Vector2(1.5f, 3f);
        public float wanderZoneRadius = 5f;
    }
}