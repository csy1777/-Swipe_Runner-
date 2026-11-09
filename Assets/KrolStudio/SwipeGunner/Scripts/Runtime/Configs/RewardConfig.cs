using System.Collections.Generic;
using UnityEngine;

namespace KrolStudio
{
    [CreateAssetMenu(fileName = "RewardConfig", menuName = "Configs/RewardConfig")]
    public class RewardConfig : ScriptableObject
    {
        [SerializeField] List<RewardLevel> rewardSpritesByLevel;

        private Dictionary<int, RewardLevel> lookup;

        /// <summary>
        /// Initializes the dictionary for quick lookup.
        /// Can be called manually or lazily upon the first GetReward.
        /// </summary>
        public void Init()
        {
            lookup = new Dictionary<int, RewardLevel>();

            if (rewardSpritesByLevel == null || rewardSpritesByLevel.Count == 0)
            {
                Debug.LogWarning($"[RewardConfig] rewardSpritesByLevel is empty in {name}", this);
                return;
            }

            foreach (var reward in rewardSpritesByLevel)
            {
                if (lookup.ContainsKey(reward.rewardLevel))
                {
                    Debug.LogError($"[RewardConfig] Duplicate rewardLevel {reward.rewardLevel} in {name}", this);
                    continue;
                }

                lookup[reward.rewardLevel] = reward;
            }
        }

        /// <summary>
        /// Get RewardLevel based on the player's level.
        /// </summary>
        public RewardLevel GetReward(int level)
        {
            if (lookup == null) Init();

            if (lookup != null && lookup.TryGetValue(level, out var reward))
                return reward;

            Debug.LogWarning($"[RewardConfig] RewardLevel {level} not found, returning first element.", this);

            // The first element is returned when the rewards are over.
            return rewardSpritesByLevel != null && rewardSpritesByLevel.Count > 0 ? rewardSpritesByLevel[0] : null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (rewardSpritesByLevel == null || rewardSpritesByLevel.Count == 0)
            {
                Debug.LogWarning($"[RewardConfig] rewardSpritesByLevel is empty in {name}", this);
                return;
            }

            var seenLevels = new HashSet<int>();
            foreach (var reward in rewardSpritesByLevel)
            {
                if (!seenLevels.Add(reward.rewardLevel))
                    Debug.LogError($"[RewardConfig] Duplicate rewardLevel {reward.rewardLevel} in {name}", this);
            }
        }
#endif

        [System.Serializable]
        public class RewardLevel
        {
            public string message;
            public int rewardLevel;
            public List<Sprite> reward;
        }
    }
}