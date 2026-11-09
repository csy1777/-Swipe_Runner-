using System.Collections.Generic;
using UnityEngine;

namespace KrolStudio
{
    [CreateAssetMenu(fileName = "RewardConfig", menuName = "Configs/RewardConfig")]
    public class RewardConfig : ScriptableObject
    {
        [SerializeField] List<RewardLevel> rewardSpritesByLevel;

        [Header("Chips / 关卡结算")]
        [Tooltip("每通关一关的芯片底薪。")]
        public int chipsPerLevelClear = 40;

        [Tooltip("每多少关算一次『奖励关』（5 = 第 5、10、15… 关）。0 = 关闭翻倍。")]
        public int rewardLevelInterval = 5;

        [Tooltip("奖励关的芯片倍率。")]
        public float rewardLevelChipsMultiplier = 2f;

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