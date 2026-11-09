using GoogleMobileAds.Ump.Api;
using System.Collections.Generic;
using UnityEngine;

namespace KrolStudio
{
    [System.Serializable]
    public class ConsentDataContainer
    {
        public bool IsUMPEnabled = true;

        [Header("UMP Debug Settings")]
        [Tooltip("Enable for tests ONLY")]
        public bool DebugMode = false;

        public DebugGeography DebugGeography = DebugGeography.Disabled;

        [Tooltip("Test Device Hashes")]
        public List<string> TestDeviceHashedIds = new List<string>();
    }
}

