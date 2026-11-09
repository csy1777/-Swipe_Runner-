using System;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    // Stores the level and type of the part, checks level availability, updates the level.
    public class PartLevelController : MonoBehaviour
    {
        [SerializeField] PartInfo info;   // Array of parts by levels

        private int upgradeLevel;

        private int level;
        public int Level => level;
     
        private PartType partType;
        public PartType PartType => partType;

        public Action OnLevelChanged;

        private IPersistentProgressService progressService;
        private PartUpgradeLevelConfig config;

        [Inject]
        void Construct(IPersistentProgressService progressService, PartUpgradeLevelConfig config)
        {
            this.progressService = progressService;
            this.config = config;
        }

        public void Initialize(PartType partType, int level)
        {
            this.partType = partType;
            this.level = level;

            upgradeLevel = config.GetLevelForUpgrade(partType, level);

            HideParts();

            OnLevelChanged?.Invoke();
        }

        void HideParts()
        {
            try
            {
                foreach (var item in info.parts)
                {
                    foreach(var part in item.parts)
                    {
                        part.SetActive(false);
                    }
                }
            }
            catch 
            {
                if (gameObject.layer == LayerMask.NameToLayer("Part"))
                {
                    Debug.LogError("Part prefab is not initialized. Check the PartLevelController component.", this);
                }
                else
                {
                    Debug.LogError("Player Context is not initialized. Check the PartLevelController component.", this);
                }
            }
        }

        public /*PartLevel*/ GameObject FindCurrentPart()
        {
            foreach (var item in info.parts)
            {
                if (item.partType == partType)
                {
                    return (level < 0) ? item.parts[0] : item.parts[level];
                }
            }

            return null;
        }

        // Check the boundaries of the part type array. For example, currently there are only 4 parts in each type.
        // Upgrading a part depends on the global level.
        public bool CanUpgrade()
        {
            foreach (var item in info.parts)
            {
                if (item.partType == partType) 
                {
                    //int partIndex = level < 0 ? 0 : level;

                    // level + 1 - check if the level can be increased
                    return level + 1 < item.parts.Length && upgradeLevel <= progressService.Progress.CurrentLevelIndex; 
                }
            }
            return false;
        }
    }
}