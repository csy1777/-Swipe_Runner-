using System.Collections.Generic;
using UnityEngine;

namespace KrolStudio
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Configs/PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [Min(0)] public int baseHealth = 50;
        [Min(0)] public int healthGrowthPerLevel = 50;
        [Range(0f, 1f)] public float lowHealthThreshold = 0.2f;

        [Space]
        [Min(1f)] public float startSpeed = 9f;
        [Min(1f)] public float acceleration = 3f;
        [Min(1f)] public float baseMoveSpeed = 7;
        [Min(0f)] public float speedGrowthPerLevel = 2;

        [Space]
        public PunchConfig punchSettings;     
    }

    [System.Serializable]
    public class PunchConfig
    {
        [Min(0f)] public float duration = 0.2f;
        [Min(0)] public int vibrato = 0;
        [Range(0, 1)] public float elasticity = 1f;
        public List<Vector3> punches;

        public Vector3 GetRandomPunch()
        {
            if (punches != null && punches.Count > 0)
                return punches[Random.Range(0, punches.Count)];
            else return Vector3.zero;
        }
    }
}