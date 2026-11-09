using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class UpgradeLevelController : MonoBehaviour, IProgressSaver, IProgressReader
    {
        private UpgradeLevelSwitcher[] levelSwitchers;
        private CtsHelper ctsHelper = new();

        private Dictionary<PartType, UpgradeLevelSwitcher> switchersByType;

        public UpgradeLevelSwitcher[] LevelSwitchers
        {
            get 
            {
                if (levelSwitchers == null)
                {
                    levelSwitchers = GetComponentsInChildren<UpgradeLevelSwitcher>();
                }

                return levelSwitchers; 
            }
        }

        [Inject]
        void Construct(ISaveLoadService saveLoadService)
        {
            saveLoadService.Register(SaveKey.UpgradeLevelController, this);
        }

        private void Awake()
        {
            switchersByType = new Dictionary<PartType, UpgradeLevelSwitcher>();
            foreach (var sw in LevelSwitchers)
            {
                if (!switchersByType.ContainsKey(sw.PartType))
                    switchersByType.Add(sw.PartType, sw);
            }
        }

        public int GetLevel(PartType partType)
        {
            if(switchersByType.TryGetValue(partType, out var switcher))
            {
                return switcher.PlayerPartLevel;
            }

            Debug.LogWarning($"[UpgradeLevelController] PartType {partType} not found!");
            return -1;
        }

        public void SetLevel(int level, PartType partType)
        {
            if(switchersByType.TryGetValue(partType, out var switcher))
            {
                switcher.Initialize(level);
            }
        }

        public bool CanUpgradePlayerPart(PartType partType)
        {
            if (switchersByType.TryGetValue(partType, out var switcher))
                return switcher.CanUpgradePlayerPart();
            return false;
        }

        public void DisplayLevelIndicators(bool value)
        {
            foreach (var part in LevelSwitchers)
            {
                part.DisplayLevelIndicator(value);
            }
        }

        public void PlayUpgradeEffect(PartType partType)
        {
            if (switchersByType.TryGetValue(partType, out var switcher))
                switcher.PlayUpgradeEffect();
        }

        public void SetBacklight(PartType partType, int partLevel, bool enable)
        {
            if (switchersByType.TryGetValue(partType, out var switcher))
            {
                if (switcher.PlayerPartLevel < 0)
                {
                    if (enable) switcher.EnablePreviewBacklight();
                    else switcher.DisablePreviewBacklight();
                }
                else
                {
                    if (enable) switcher.EnableBacklight(partLevel);
                    else switcher.DisableBacklight();
                }
            }
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            Dictionary<PartType, int> datas = new();

            foreach (var part in LevelSwitchers)
            {
                datas.Add(part.PartType, part.PlayerPartLevel);
            }

            progress.PlayerData = new PlayerData(datas);
        }

        public void LoadProgress(PlayerProgress progress)
        {
            Dictionary<PartType, int> savedValues = progress.PlayerData.ToDictionary();
            foreach (var item in savedValues)
            {
                PartType partType = item.Key;
                int level = item.Value;
                ApplyPartLevelDelayed(partType, level).Forget();
            }
        }

        private async UniTask ApplyPartLevelDelayed(PartType partType, int level)
        {
            try
            {
                await UniTask.NextFrame(ctsHelper.Token);   // wait for the next frame
                SetLevel(level, partType);                  // safely use after await
            }
            catch (OperationCanceledException)
            {
            }
            catch (MissingReferenceException ex)
            {
                Debug.LogWarning($"The object was destroyed during animation: {ex.Message}", this);
            }
            catch (NullReferenceException ex)
            {
                Debug.LogWarning($"Missing link during animation: {ex.Message}", this);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error: {ex}", this);
            }
        }
    }
}

