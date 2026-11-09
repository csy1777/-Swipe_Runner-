#if LEVELPLAY_DEPENDENCIES_INSTALLED

using Unity.Services.LevelPlay;
using UnityEngine;

namespace KrolStudio
{
    public class LevelPlayHandler : AdProviderHandler
    {
        private LevelPlayBannerAd bannerAd;
        private LevelPlayInterstitialAd interstitialAd;
        private LevelPlayRewardedAd rewardedAd;

        private bool interstitialLoaded;
        private bool rewardedLoaded;

        public LevelPlayHandler(AdProvider providerType) : base(providerType) { }

        //-----------------------------------------------------------------------------------------
        #region Initialize

        public override void Initialize(AdsConfig adsConfig, AdsManager adsManager)
        {
            this.adsConfig = adsConfig;
            this.adsManager = adsManager;

            if (adsConfig.Logging)
                Debug.Log("LevelPlay initializing");

#if UNITY_ANDROID
            string appKey = adsConfig.LevelPlayContainer.AndroidAppKey;
#elif UNITY_IOS
            string appKey = adsConfig.LevelPlayContainer.IOSAppKey;
#else
            string appKey = string.Empty;
#endif

            if (string.IsNullOrEmpty(appKey))
            {
                Debug.LogError("LevelPlay appKey is empty");
                return;
            }

            LevelPlay.ValidateIntegration();

            LevelPlay.OnInitSuccess += OnSdkInitialized;
            LevelPlay.OnInitFailed += OnSdkInitializationFailed;

            LevelPlay.Init(appKey);
        }

        private void OnSdkInitialized(LevelPlayConfiguration config)
        {
            if (adsConfig.Logging)
                Debug.Log("LevelPlay SDK initialized");

            RegisterAds();
            OnProviderInitialised();
        }

        private void OnSdkInitializationFailed(LevelPlayInitError error)
        {
            Debug.LogError($"LevelPlay init failed: {error}");
        }

        #endregion

        //-----------------------------------------------------------------------------------------
        #region Register Ads

        private void RegisterAds()
        {
            // ================= Rewarded =================
            rewardedAd = new LevelPlayRewardedAd(GetRewardedVideoID());

            rewardedAd.OnAdLoaded += adInfo =>
            {
                rewardedLoaded = true;
                adsManager.OnProviderAdLoaded(providerType, AdType.RewardedVideo);
            };

            rewardedAd.OnAdLoadFailed += error =>
            {
                rewardedLoaded = false;
            };

            rewardedAd.OnAdDisplayed += adInfo =>
            {
                adsManager.OnProviderAdDisplayed(providerType, AdType.RewardedVideo);
            };

            rewardedAd.OnAdRewarded += (adInfo, reward) =>
            {
                adsManager.ExecuteRewardVideoCallback(true);
            };

            rewardedAd.OnAdClosed += adInfo =>
            {
                adsManager.OnProviderAdClosed(providerType, AdType.RewardedVideo);
                rewardedLoaded = false;
                rewardedAd.LoadAd();
            };

            rewardedAd.LoadAd();

            // ================= Interstitial =================
            interstitialAd = new LevelPlayInterstitialAd(GetInterstitialID());

            interstitialAd.OnAdLoaded += adInfo =>
            {
                interstitialLoaded = true;
                adsManager.OnProviderAdLoaded(providerType, AdType.Interstitial);
            };

            interstitialAd.OnAdLoadFailed += error =>
            {
                interstitialLoaded = false;
            };

            interstitialAd.OnAdDisplayed += adInfo =>
            {
                adsManager.OnProviderAdDisplayed(providerType, AdType.Interstitial);
            };

            interstitialAd.OnAdClosed += adInfo =>
            {
                interstitialLoaded = false;

                adsManager.OnProviderAdClosed(providerType, AdType.Interstitial);
                adsManager.ExecuteInterstitialCallback(true);

                adsManager.ResetInterstitialDelayTime();
                interstitialAd.LoadAd();
            };

            interstitialAd.LoadAd();

            // ================= Banner =================
            bannerAd = new LevelPlayBannerAd(GetBannerID());

            bannerAd.OnAdLoaded += adInfo =>
            {
                adsManager.OnProviderAdLoaded(providerType, AdType.Banner);
            };

            bannerAd.OnAdLoadFailed += error => { };
        }

        #endregion

        //-----------------------------------------------------------------------------------------
        #region Requests

        public override void RequestInterstitial()
        {
            interstitialAd?.LoadAd();
        }

        public override void RequestRewardedVideo()
        {
            rewardedAd?.LoadAd();
        }

        #endregion

        //-----------------------------------------------------------------------------------------
        #region Show

        public override void ShowBanner()
        {
            bannerAd?.LoadAd();
        }

        public override void HideBanner()
        {
            bannerAd?.HideAd();
        }

        public override void DestroyBanner()
        {
            bannerAd?.DestroyAd();
        }

        public override void ShowInterstitial(InterstitialCallback callback)
        {
            if (!IsInterstitialLoaded())
            {
                adsManager.ExecuteInterstitialCallback(false);
                return;
            }

            interstitialAd.ShowAd();
        }

        public override void ShowRewardedVideo(RewardedVideoCallback callback)
        {
            if (!IsRewardedVideoLoaded())
            {
                adsManager.ExecuteRewardVideoCallback(false);
                return;
            }

            rewardedAd.ShowAd();
        }

        #endregion

        //-----------------------------------------------------------------------------------------
        #region State

        public override bool IsInterstitialLoaded()
        {
            return interstitialLoaded &&
                   interstitialAd != null &&
                   interstitialAd.IsAdReady();
        }

        public override bool IsRewardedVideoLoaded()
        {
            return rewardedLoaded &&
                   rewardedAd != null &&
                   rewardedAd.IsAdReady();
        }

        #endregion

        //-----------------------------------------------------------------------------------------
        #region IDs

        public string GetBannerID()
        {
#if UNITY_ANDROID
            return adsConfig.LevelPlayContainer.AndroidBannerID;
#elif UNITY_IOS
            return adsConfig.LevelPlayContainer.IOSBannerID;
#else
            return "unexpected_platform";
#endif
        }

        public string GetInterstitialID()
        {
#if UNITY_ANDROID
            return adsConfig.LevelPlayContainer.AndroidInterstitialID;
#elif UNITY_IOS
            return adsConfig.LevelPlayContainer.IOSInterstitialID;
#else
            return "unexpected_platform";
#endif
        }

        public string GetRewardedVideoID()
        {
#if UNITY_ANDROID
            return adsConfig.LevelPlayContainer.AndroidRewardedVideoID;
#elif UNITY_IOS
            return adsConfig.LevelPlayContainer.IOSRewardedVideoID;
#else
            return "unexpected_platform";
#endif
        }

        #endregion
    }
}

#endif