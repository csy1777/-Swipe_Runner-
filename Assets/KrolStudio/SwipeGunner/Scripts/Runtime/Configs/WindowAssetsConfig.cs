using System;
using System.Collections.Generic;
using UnityEngine;

namespace KrolStudio
{
    [CreateAssetMenu(fileName = "WindowAssetsConfig", menuName = "Configs/WindowAssetsConfig")]
    public class WindowAssetsConfig : ScriptableObject
    {
        public List<WindowAsset> WindowAssets;
    }

    [Serializable]
    public class WindowAsset
    {
        public WindowType WindowType;   // Window type(e.g., Upgrade, Reward, Pause)
        public GameObject WindowPrefab; // Window prefab for Instantiate
    }
}