using System.Collections.Generic;
using UnityEngine;

namespace KrolStudio
{
    [CreateAssetMenu(fileName = "PartUpgradeLevel", menuName = "Configs/PartUpgradeLevel")]
    public class PartUpgradeLevelConfig : ScriptableObject
    {
        public List<PartConfig> config;

        private Dictionary<PartType, Dictionary<int, int>> lookup;

        public void Init()
        {
            lookup = new Dictionary<PartType, Dictionary<int, int>>();
            foreach (var item in config)
            {
                var dict = new Dictionary<int, int>();
                foreach (var part in item.parts)
                    dict[part.partLevel] = part.upgradeLevel;

                lookup[item.partType] = dict;
            }
        }

        public int GetLevelForUpgrade(PartType partType, int partLevel)
        {
            if (lookup == null)
                Init();

            if (lookup.TryGetValue(partType, out var levels) &&
                levels.TryGetValue(partLevel, out var upgradeLevel))
            {
                return upgradeLevel;
            }

            return -1;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            Init();

            if (config == null || config.Count == 0)
            {
                Debug.LogWarning($"[PartUpgradeLevelConfig] Config is empty in {name}", this);
                return;
            }

            var partTypes = new HashSet<PartType>();

            foreach (var partConfig in config)
            {
                if (!partTypes.Add(partConfig.partType))
                {
                    Debug.LogError(
                        $"[PartUpgradeLevelConfig] Duplicate PartType {partConfig.partType} " +
                        $"in the object {name}", this);
                }

                var seenLevels = new HashSet<int>();
                foreach (var part in partConfig.parts)
                {
                    if (!seenLevels.Add(part.partLevel))
                    {
                        Debug.LogError(
                            $"[PartUpgradeLevelConfig] Duplicate level {part.partLevel} " +
                            $"in PartType {partConfig.partType} in the object {name}", this);
                    }
                }
            }
        }
#endif

    }

    [System.Serializable]
    public class PartConfig
    {
        public PartType partType;
        public PartLevelUpConfig[] parts;
    }

    [System.Serializable]
    public class PartLevelUpConfig
    {
        [Range(0, 10)] public int partLevel;
        [Min(1)] public int upgradeLevel;
    }
}