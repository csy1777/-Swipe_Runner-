using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace KrolStudio
{
    public class AdsInitializer
    {
        private const int INIT_ATTEMPTS_AMOUNT = 30;

        private AdsConfig config;
        private AdProviderRegistry registry;
        private ConsentManager consentManager;
        private IDFAManager idfaManager;
        private InterstitialAdController interstitialController;
        private RewardedAdController rewardedController;
        private BannerAdController bannerController;
        private IPersistentProgressService progressService;

        private bool isFirstAdLoaded = false;
        public bool IsFirstAdLoaded => isFirstAdLoaded;

        public void Initialize(
            AdsConfig config,
            AdProviderRegistry registry,
            ConsentManager consentManager,
            IDFAManager idfaManager,
            InterstitialAdController interstitialController,
            RewardedAdController rewardedController,
            BannerAdController bannerController,
            IPersistentProgressService progressService)
        {
            this.config = config;
            this.registry = registry;
            this.consentManager = consentManager;
            this.idfaManager = idfaManager;
            this.interstitialController = interstitialController;
            this.rewardedController = rewardedController;
            this.bannerController = bannerController;
            this.progressService = progressService;

            consentManager.ConsentStateChanged += OnConsentStateChanged;
        }

        private void OnConsentStateChanged(ConsentManager.ConsentState state)
        {
            ApplyConsentToProviders();
            TryToLoadAds().Forget();
        }
  
        private void ApplyConsentToProviders()
        {
            bool personalizedAds = consentManager.CanShowPersonalizedAds;
#if MODULE_ADMOB
            var admobHandler = registry.GetHandler(AdProvider.AdMob) as AdMobHandler;
            if (admobHandler != null)
            {
                admobHandler.SetNonPersonalizedAds(!personalizedAds);
            }
#endif
        }

        public async UniTask TryToLoadAds()
        {
            int initAttempts = 0;

            await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false);

            while (!isFirstAdLoaded && initAttempts < INIT_ATTEMPTS_AMOUNT)
            {
                if (LoadAds())
                    break;

                await UniTask.Delay(TimeSpan.FromSeconds(1.0 * (initAttempts + 1)), ignoreTimeScale: false);
                initAttempts++;
            }

            if (config.Logging)
                Debug.Log("[AdsInitializer]: First ads loaded!");
        }

        private bool LoadAds()
        {
            if (isFirstAdLoaded)
                return true;

            if (config.IsIDFAEnabled && !idfaManager.IsIDFADetermined())
                return false;

            bool rewardedReady = CheckAdTypeReady(config.RewardedVideoType);
            bool interstitialReady = CheckAdTypeReady(config.InterstitialType);
            bool bannerReady = CheckAdTypeReady(config.BannerType);

            if (rewardedReady && interstitialReady && bannerReady)
            {
                if (config.RewardedVideoType != AdProvider.Disable)
                    rewardedController.Request();

                bool forcedAdEnabled = progressService.Progress.IsForcedAdEnabled;

                if (config.InterstitialType != AdProvider.Disable && forcedAdEnabled)
                    interstitialController.Request();

                if (config.BannerType != AdProvider.Disable && forcedAdEnabled)
                    bannerController.Show();

                isFirstAdLoaded = true;
                return true;
            }

            return false;
        }

        private bool CheckAdTypeReady(AdProvider provider)
        {
            if (provider == AdProvider.Disable)
                return true;

            return registry.IsInitialised(provider);
        }
    }
}