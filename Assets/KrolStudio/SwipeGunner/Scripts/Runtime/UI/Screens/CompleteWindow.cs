using Zenject;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace KrolStudio
{
    public class CompleteWindow : WindowBase
    {
        [SerializeField] Transform rewardHolder;
        [SerializeField] Image rewardIconPrefab;
        [SerializeField] TextMeshProUGUI rewardMessage;

        ISceneLoader sceneLoader;
        ISaveLoadService saveLoadService;
        LevelPlacer levelPlacer;
        RewardConfig rewardConfig;
        IPersistentProgressService progressService;
        AdsManager adsManager;

        List<Image> rewardIcons = new();

        bool adWasShown;

        [Inject]
        void Construct(ISceneLoader sceneLoader, 
                       LevelPlacer levelPlacer, 
                       ISaveLoadService saveLoadService,
                       RewardConfig rewardConfig,
                       IPersistentProgressService progressService,
                       AdsManager adsManager)
        {
            this.sceneLoader = sceneLoader;
            this.levelPlacer = levelPlacer;
            this.saveLoadService = saveLoadService;
            this.rewardConfig = rewardConfig;
            this.progressService = progressService;
            this.adsManager = adsManager;
        }

        protected override void OnEnable()
        {
            base.OnEnable();         
            InitializeCompleteScreen();
            levelPlacer.IncrementLevelIndex();
            saveLoadService.SaveProgress(SaveKey.All);
        }

        private void InitializeCompleteScreen()
        {
            RewardConfig.RewardLevel reward = rewardConfig.GetReward(progressService.Progress.CurrentLevelIndex);

            if (reward == null)
            {
                Debug.LogWarning("Failed to issue rewards, please check your RewardConfig configuration.");
                return;
            }

            rewardMessage.text = reward.message;

            foreach (var sprite in reward.reward)
            {
                rewardIcons.Add(Instantiate(rewardIconPrefab, rewardHolder));
                rewardIcons[^1].sprite = sprite;
            }
        }

        private void RemoveRewardsIcons()
        {
            foreach (var image in rewardIcons)
            {
                Object.Destroy(image.gameObject);
            }
        }

        protected override void CloseWindow()
        {
            if (!adWasShown && adsManager.Settings.InterstitialType != AdProvider.Disable)
            {
                adWasShown = true;
                adsManager.ShowInterstitial(null, true);
            }
            else
            {
                NextLevel();
                RemoveRewardsIcons();
            }
        }

        void NextLevel()
        {    
            sceneLoader.Load(SceneName.Level.ToString());
        }

        protected override void Cleanup() { }

        protected override void Initialize() { }

        protected override void SubscribeUpdates() { }
    }
}