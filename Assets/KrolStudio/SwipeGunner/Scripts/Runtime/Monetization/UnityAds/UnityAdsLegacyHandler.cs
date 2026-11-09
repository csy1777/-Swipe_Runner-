#if MODULE_UNITYADS

using UnityEngine;
using UnityEngine.Advertisements;

namespace KrolStudio
{
    public class UnityAdsLegacyHandler : AdProviderHandler
    {
        public string PlacementBannerID { get; private set; }
        public string PlacementInterstitialID { get; private set; }
        public string PlacementRewardedVideoID { get; private set; }
        public string AppId { get; private set; }

        public bool interstitialIsLoaded;
        public bool rewardVideoIsLoaded;

        public int initializationAttemptCount = 0;

        public UnityAdvertismentListener unityAdvertisment;

        public UnityAdsLegacyHandler(AdProvider moduleType) : base(moduleType) { }

        public override void Initialize(AdsConfig adsConfig, AdsManager adsManager)
        {
            this.adsConfig = adsConfig;
            this.adsManager = adsManager;

            if (isInitialised || Advertisement.isInitialized)
            {
                Debug.LogError("Unity Ads Legacy is already initialized!");

                return;
            }

            if (!Advertisement.isSupported)
            {
                Debug.LogError("Unity Ads Legacy isn't supported!");

                return;
            }

            PlacementBannerID = GetBannerID();
            PlacementInterstitialID = GetInterstitialID();
            PlacementRewardedVideoID = GetRewardedVideoID();
            AppId = GetUnityAdsAppID();

            unityAdvertisment = GetUnityAdvertismentListener();
            Object.DontDestroyOnLoad(unityAdvertisment);

            unityAdvertisment.Init(adsConfig, this, adsManager);

            Advertisement.Initialize(AppId, adsConfig.DebugMode, unityAdvertisment);
            Advertisement.Banner.SetPosition((UnityEngine.Advertisements.BannerPosition)adsConfig.UnityAdsContainer.BannerPosition);

            if (adsConfig.Logging)
            {
                Debug.Log("Unity Ads initialized: " + Advertisement.isInitialized);
                Debug.Log("Unity Ads is supported: " + Advertisement.isSupported);
                Debug.Log("Unity Ads test mode enabled: " + Advertisement.debugMode);
                Debug.Log("Unity Ads version: " + Advertisement.version);
            }

            OnProviderInitialised();
        }

        private UnityAdvertismentListener GetUnityAdvertismentListener()
        {
            GameObject unityAdListener = new GameObject("UnityAdListener");
            return unityAdListener.AddComponent<UnityAdvertismentListener>();
        }

        public override void DestroyBanner()
        {
            Advertisement.Banner.Hide(true);
        }

        public override void HideBanner()
        {
            Advertisement.Banner.Hide(false);
        }

        public override void RequestInterstitial()
        {
            Advertisement.Load(PlacementInterstitialID, unityAdvertisment);
        }

        public override void RequestRewardedVideo()
        {
            Advertisement.Load(PlacementRewardedVideoID, unityAdvertisment);
        }

        public override void ShowBanner()
        {
            Advertisement.Banner.Show(PlacementBannerID);
        }

        public override void ShowInterstitial(InterstitialCallback callback)
        {
            Advertisement.Show(PlacementInterstitialID, unityAdvertisment);
        }

        public override void ShowRewardedVideo(RewardedVideoCallback callback)
        {
            Advertisement.Show(PlacementRewardedVideoID, unityAdvertisment);
        }

        public override bool IsInterstitialLoaded()
        {
            return interstitialIsLoaded;
        }

        public override bool IsRewardedVideoLoaded()
        {
            return rewardVideoIsLoaded;
        }

        public string GetUnityAdsAppID()
        {
#if UNITY_ANDROID
            return adsConfig.UnityAdsContainer.AndroidAppID;
#elif UNITY_IOS
            return adsConfig.UnityAdsContainer.IOSAppID;
#else
            return string.Empty;
#endif
        }

        public string GetBannerID()
        {
#if UNITY_ANDROID
            return adsConfig.UnityAdsContainer.AndroidBannerID;
#elif UNITY_IOS
            return adsConfig.UnityAdsContainer.IOSBannerID;
#else
            return string.Empty;
#endif
        }

        public string GetInterstitialID()
        {
#if UNITY_ANDROID
            return adsConfig.UnityAdsContainer.AndroidInterstitialID;
#elif UNITY_IOS
            return adsConfig.UnityAdsContainer.IOSInterstitialID;
#else
            return string.Empty;
#endif
        }

        public string GetRewardedVideoID()
        {
#if UNITY_ANDROID
            return adsConfig.UnityAdsContainer.AndroidRewardedVideoID;
#elif UNITY_IOS
            return adsConfig.UnityAdsContainer.IOSRewardedVideoID;
#else
            return string.Empty;
#endif
        }
    }
}
#endif