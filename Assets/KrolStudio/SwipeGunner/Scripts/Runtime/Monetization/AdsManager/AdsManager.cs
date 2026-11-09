using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;


namespace KrolStudio
{
    public class AdsManager : ITickable
    {
        private bool isModuleInitialised;
        private AdsConfig settings;

        private AdProviderFactory factory;
        private AdProviderRegistry registry;
        private ConsentManager consentManager;
        private IDFAManager idfaManager;
        private InterstitialAdController interstitialController;
        private RewardedAdController rewardedController;
        private BannerAdController bannerController;
        private AdsInitializer initializer;
        private MainThreadEventDispatcher eventDispatcher;

        private IPersistentProgressService progressService;

        // Events
        public event Action ForcedAdDisabled;
        public event Action<AdProvider> AdProviderInitialised
        {
            add => registry.ProviderInitialised += value;
            remove => registry.ProviderInitialised -= value;
        }
        public event Action<AdProvider, AdType> AdLoaded
        {
            add => registry.AdLoaded += value;
            remove => registry.AdLoaded -= value;
        }
        public event Action<AdProvider, AdType> AdDisplayed
        {
            add => registry.AdDisplayed += value;
            remove => registry.AdDisplayed -= value;
        }
        public event Action<AdProvider, AdType> AdClosed
        {
            add => registry.AdClosed += value;
            remove => registry.AdClosed -= value;
        }

        public Action InterstitialConditions
        {
            get => interstitialController.InterstitialConditions;
            set => interstitialController.InterstitialConditions = value;
        }

        public AdsConfig Settings => settings;

        [Inject]
        void Construct(
            IPersistentProgressService progressService,
            AdsConfig adsConfig)
        {
            this.progressService = progressService;
            this.settings = adsConfig;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            factory = new AdProviderFactory();
            registry = new AdProviderRegistry();
            consentManager = new ConsentManager();
            idfaManager = new IDFAManager();
            interstitialController = new InterstitialAdController();
            rewardedController = new RewardedAdController();
            bannerController = new BannerAdController();
            initializer = new AdsInitializer();
            eventDispatcher = new MainThreadEventDispatcher();
        }

        public void Initialize()
        {
            if (isModuleInitialised)
            {
                Debug.LogWarning("[AdsManager]: Module already exists!");
                return;
            }

            if (settings == null)
            {
                Debug.LogError("[AdsManager]: Settings don't exist!");
                return;
            }

            isModuleInitialised = true;

            registry.Initialize(settings, factory);
            consentManager.Initialize(settings);
            idfaManager.Initialize(settings);
            interstitialController.Initialize(settings, registry, progressService);
            rewardedController.Initialize(settings, registry);
            bannerController.Initialize(settings, registry, progressService);

            initializer.Initialize(
                settings,
                registry,
                consentManager,
                idfaManager,
                interstitialController,
                rewardedController,
                bannerController,
                progressService);

            registry.InitializeHandlers(settings, this);

            if (settings.LoadAdsOnStart)
            {
                initializer.TryToLoadAds().Forget();
            }
        }

        public void Tick()
        {
            if (!initializer.IsFirstAdLoaded) return;

            eventDispatcher.ProcessEvents();

            if (settings.AutoShowInterstitial && interstitialController.CheckTime())
            {
                ShowInterstitial(null);
                interstitialController.ResetDelayTime();
            }
        }

        public void CallEventInMainThread(Action callback)
        {
            eventDispatcher.Enqueue(callback);
        }

        // Interstitial
        public bool IsInterstitialLoaded() =>
            isModuleInitialised && settings.IsModuleActive && interstitialController.IsLoaded();

        public void RequestInterstitial() =>
            interstitialController.Request();

        public void ShowInterstitial(AdProviderHandler.InterstitialCallback callback, bool ignoreConditions = false) =>
            interstitialController.Show(callback, ignoreConditions);

        public void ExecuteInterstitialCallback(bool result) =>
            interstitialController.ExecuteCallback(result);

        public void SetInterstitialDelayTime(float time) =>
            interstitialController.SetDelayTime(time);

        public void ResetInterstitialDelayTime() =>
            interstitialController.ResetDelayTime();

        // Rewarded
        public bool IsRewardBasedVideoLoaded() =>
            isModuleInitialised && settings.IsModuleActive && rewardedController.IsLoaded();

        public void RequestRewardBasedVideo() =>
            rewardedController.Request();

        public void ShowRewardBasedVideo(AdProviderHandler.RewardedVideoCallback callback, bool showErrorMessage = true) =>
            rewardedController.Show(callback, showErrorMessage);

        public void ExecuteRewardVideoCallback(bool result) =>
            rewardedController.ExecuteCallback(result);

        // Banner
        public void ShowBanner() => bannerController.Show();
        public void HideBanner() => bannerController.Hide();
        public void DestroyBanner() => bannerController.Destroy();
        public void EnableBanner() => bannerController.Enable();
        public void DisableBanner() => bannerController.Disable();

        // Provider events (called from handlers)
        public void OnProviderInitialised(AdProvider provider) =>
            registry.OnProviderInitialised(provider);

        public void OnProviderAdLoaded(AdProvider provider, AdType adType) =>
            registry.OnAdLoaded(provider, adType);

        public void OnProviderAdDisplayed(AdProvider provider, AdType adType)
        {
            registry.OnAdDisplayed(provider, adType);

            if (adType == AdType.Interstitial || adType == AdType.RewardedVideo)
                interstitialController.ResetDelayTime();
        }

        public void OnProviderAdClosed(AdProvider provider, AdType adType)
        {
            registry.OnAdClosed(provider, adType);

            if (adType == AdType.Interstitial || adType == AdType.RewardedVideo)
                interstitialController.ResetDelayTime();
        }

        public void DisableForcedAd()
        {
            Debug.Log("[AdsManager]: Banners and interstitials are disabled!");

            progressService.Progress.IsForcedAdEnabled = false;
            ForcedAdDisabled?.Invoke();
            DestroyBanner();
        }

        public bool CanShowPersonalizedAds() => consentManager.CanShowPersonalizedAds;
        public bool IsIDFADetermined() => idfaManager.IsIDFADetermined();
    }
}

/*
Separation of concerns:

1. AdProviderFactory - provider creation
2. ConsentManager - consent management (GDPR/UMP)
3. IDFAManager - IDFA management (iOS)
4. InterstitialAdController - interstitial advertising logic
5. RewardedAdController - rewarded advertising logic
6. BannerAdController - banner logic
7. AdProviderRegistry - provider registry and management
8. AdsInitializer - initialization and initial loading of ads
9. MainThreadEventDispatcher - main thread event handling
10. AdsManager - coordinator delegating work to specialized classes

Each class now has one clear responsibility!
*/