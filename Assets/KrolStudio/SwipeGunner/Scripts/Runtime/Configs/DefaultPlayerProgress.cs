using System.Collections.Generic;
using UnityEngine;

namespace KrolStudio
{
    [CreateAssetMenu(fileName = "DefaultPlayerProgress", menuName = "Configs/DefaultPlayerProgress")]
    public class DefaultPlayerProgress : ScriptableObject
    {
        [Min(1)] public int currentLevelIndex = 1;

        public List<CurrencySettings> currencySettings;
        public List<PartTypeSettings> partSettings;

        [Header("Upgrade")]
        [Min(1)] public int incomeLevel = 1;
        [Min(1)] public int addPartlevel = 1;

        [Header("Settings")]
        public bool vibrate = false;
        [Range(0.0001f, 1f)] public float musicVolume = 0.5f;
        [Range(0.0001f, 1f)] public float sfxVolume = 0.5f;

        [Header("Settings")]
        public bool isForcedAdEnabled = true;

        [System.Serializable]
        public class PartTypeSettings
        {
            public PartType type;
            [Range(-1, 10)] public int level;
        }

        [System.Serializable]
        public class CurrencySettings
        {
            public CurrencyType type;
            [Min(0)] public int count;
        }
    }
}