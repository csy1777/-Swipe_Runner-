#if MODULE_ADMOB

using UnityEngine;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using Cysharp.Threading.Tasks;
using System;

namespace KrolStudio
{
    public class AdMobHandler : AdProviderHandler
    {
        private const int RETRY_ATTEMPT_DEFAULT_VALUE = 1;

        private int interstitialRetryAttempt = RETRY_ATTEMPT_DEFAULT_VALUE;
        private int rewardedRetryAttempt = RETRY_ATTEMPT_DEFAULT_VALUE;

        private BannerView bannerView;
        private InterstitialAd interstitial;
        private RewardedAd rewardBasedVideo;

        public AdMobHandler(AdProvider providerType) : base(providerType) { }

        private bool nonPersonalizedAds = false;

        public override void Initialize(AdsConfig adsConfig, AdsManager adsManager)
        {
            this.adsConfig = adsConfig;
            this.adsManager = adsManager;

            if (adsConfig.Logging)
                Debug.Log("[AdMobHandler]: AdMob is trying to initialize!");

            MobileAds.SetiOSAppPauseOnBackground(true);

            RequestConfiguration requestConfiguration = new RequestConfiguration()
            {
                TagForChildDirectedTreatment = TagForChildDirectedTreatment.Unspecified,
                TestDeviceIds = adsConfig.AdMobContainer.TestDevicesIDs
            };

            MobileAds.SetRequestConfiguration(requestConfiguration);

            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize(InitCompleteAction);
        }


        public void SetNonPersonalizedAds(bool npa)
        {
            nonPersonalizedAds = npa;
            if (adsConfig.Logging)
                Debug.Log($"[AdMobHandler]: SetNonPersonalizedAds = {nonPersonalizedAds}");
        }

        public AdRequest GetAdRequest()
        {
            var request = new AdRequest()
            {
                Extras = new Dictionary<string, string>()
                {
                    { "npa", nonPersonalizedAds ? "1" : "0" }
                }
            };

            return request;
        }


        private async UniTask DelayedCall(double delay, Action callback, bool ignoreTimeScale = false)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay), ignoreTimeScale);
            callback?.Invoke();
        }

        private void InitCompleteAction(InitializationStatus initStatus)
        {
            GoogleMobileAds.Common.MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                OnProviderInitialised();
            });
        }

        public override void DestroyBanner()
        {
            if (bannerView != null)
                bannerView.Destroy();
        }

        public override void HideBanner()
        {
            if (bannerView != null)
                bannerView.Hide();
        }


        //-----------------------------------------------------------------------------------------

        public override void RequestInterstitial()
        {
            // Clean up interstitial ad before creating a new one.
            if (interstitial != null)
            {
                interstitial.Destroy();
            }

            InterstitialAd.Load(GetInterstitialID(), GetAdRequest(), (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    if (adsConfig.Logging)
                        Debug.Log("[AdMobHandler]: Interstitial ad failed to load an ad with error: " + error);

                    interstitialRetryAttempt++;
                    float retryDelay = Mathf.Pow(2, interstitialRetryAttempt);

                    DelayedCall(interstitialRetryAttempt, () => adsManager.RequestInterstitial(), true).Forget();

                    return;
                }

                if (adsConfig.Logging)
                    Debug.Log("[AdMobHandler]: Interstitial ad loaded with response: " + ad.GetResponseInfo());

                interstitial = ad;

                interstitialRetryAttempt = RETRY_ATTEMPT_DEFAULT_VALUE;

                adsManager.OnProviderAdLoaded(providerType, AdType.Interstitial);

                // Register for ad events.
                interstitial.OnAdFullScreenContentOpened += HandleInterstitialOpened;
                interstitial.OnAdFullScreenContentClosed += HandleInterstitialClosed;
                interstitial.OnAdClicked += HandleInterstitialClicked;
            });
        }

        public override void RequestRewardedVideo()
        {
            RewardedAd.Load(GetRewardedVideoID(), GetAdRequest(), (RewardedAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    adsManager.ExecuteRewardVideoCallback(false);

                    if (adsConfig.Logging)
                        Debug.Log("[AdMobHandler]: HandleRewardBasedVideoFailedToLoad event received with message: " + error);

                    rewardedRetryAttempt++;
                    float retryDelay = Mathf.Pow(2, rewardedRetryAttempt);

                    DelayedCall(rewardedRetryAttempt, () => adsManager.RequestRewardBasedVideo(), true).Forget();

                    return;
                }

                if (adsConfig.Logging)
                    Debug.Log("[AdMobHandler]: Rewarded ad loaded with response: " + ad.GetResponseInfo());

                rewardedRetryAttempt = RETRY_ATTEMPT_DEFAULT_VALUE;

                adsManager.OnProviderAdLoaded(providerType, AdType.RewardedVideo);

                rewardBasedVideo = ad;
                rewardBasedVideo.OnAdFullScreenContentFailed += HandleRewardBasedVideoFailedToShow;
                rewardBasedVideo.OnAdFullScreenContentOpened += HandleRewardBasedVideoOpened;
                rewardBasedVideo.OnAdFullScreenContentClosed += HandleRewardBasedVideoClosed;
                rewardBasedVideo.OnAdClicked += HandleRewardBasedVideoClicked;
            });
        }

        private void RequestBanner()
        {
            // Clean up banner before reusing
            if (bannerView != null)
            {
                bannerView.Destroy();
            }

            AdSize adSize = AdSize.Banner;

            switch (adsConfig.AdMobContainer.BannerType)
            {
                case AdMobContainer.BannerPlacementType.Banner:
                    adSize = AdSize.Banner;
                    break;
                case AdMobContainer.BannerPlacementType.MediumRectangle:
                    adSize = AdSize.MediumRectangle;
                    break;
                case AdMobContainer.BannerPlacementType.IABBanner:
                    adSize = AdSize.IABBanner;
                    break;
                case AdMobContainer.BannerPlacementType.Leaderboard:
                    adSize = AdSize.Leaderboard;
                    break;
            }

            AdPosition adPosition = AdPosition.Bottom;
            switch (adsConfig.AdMobContainer.BannerPosition)
            {
                case BannerPosition.Bottom:
                    adPosition = AdPosition.Bottom;
                    break;
                case BannerPosition.Top:
                    adPosition = AdPosition.Top;
                    break;
            }

            bannerView = new BannerView(GetBannerID(), adSize, adPosition);

            // Register for ad events.
            bannerView.OnBannerAdLoaded += HandleAdLoaded;
            bannerView.OnBannerAdLoadFailed += HandleAdFailedToLoad;
            bannerView.OnAdPaid += HandleAdPaid;
            bannerView.OnAdClicked += HandleAdClicked;
            bannerView.OnAdFullScreenContentClosed += HandleAdClosed;

            // Load a banner ad.
            bannerView.LoadAd(GetAdRequest());
        }

        //-----------------------------------------------------------------------------------------

        public override void ShowBanner()
        {
            if (bannerView == null)
                RequestBanner();

            if(bannerView != null)
                bannerView.Show();
        }

        public override void ShowInterstitial(InterstitialCallback callback)
        {
            interstitial.Show();
        }

        public override void ShowRewardedVideo(RewardedVideoCallback callback)
        {
            rewardBasedVideo.Show((GoogleMobileAds.Api.Reward reward) =>
            {
                adsManager.CallEventInMainThread(delegate
                {
                    adsManager.OnProviderAdDisplayed(providerType, AdType.RewardedVideo);

                    adsManager.ExecuteRewardVideoCallback(true);

                    if (adsConfig.Logging)
                        Debug.Log("[AdMobHandler]: HandleRewardBasedVideoRewarded event received");

                    adsManager.ResetInterstitialDelayTime();
                    adsManager.RequestRewardBasedVideo();
                });
            });
        }

        //-----------------------------------------------------------------------------------------


        public override bool IsInterstitialLoaded()
        {
            return interstitial != null && interstitial.CanShowAd();
        }

        public override bool IsRewardedVideoLoaded()
        {
            return rewardBasedVideo != null && rewardBasedVideo.CanShowAd();
        }
        
        //-----------------------------------------------------------------------------------------

    #region Banner Callbacks
        public void HandleAdLoaded()
        {
            adsManager.CallEventInMainThread(delegate
            {
                if (adsConfig.Logging)
                    Debug.Log("[AdMobHandler]: HandleAdLoaded event received");

                adsManager.OnProviderAdLoaded(providerType, AdType.Banner);
            });
        }

        public void HandleAdFailedToLoad(LoadAdError error)
        {
            adsManager.CallEventInMainThread(delegate
            {
                if (adsConfig.Logging)
                    Debug.Log("[AdMobHandler]: HandleFailedToReceiveAd event received with message: " + error.GetMessage());
            });
        }

        private void HandleAdPaid(AdValue adValue)
        {
            adsManager.CallEventInMainThread(delegate
            {
                if (adsConfig.Logging)
                    Debug.Log("[AdMobHandler]: HandleAdPaid event received");
            });
        }

        public void HandleAdClicked()
        {
            adsManager.CallEventInMainThread(delegate
            {
                if (adsConfig.Logging)
                    Debug.Log("[AdMobHandler]: HandleAdClicked event received");
            });
        }

        public void HandleAdClosed()
        {
            adsManager.CallEventInMainThread(delegate
            {
                if (adsConfig.Logging)
                    Debug.Log("[AdMobHandler]: HandleAdClosed event received");

                adsManager.OnProviderAdClosed(providerType, AdType.Banner);
            });
        }
    #endregion

    #region Interstitial Callback
        public void HandleInterstitialOpened()
        {
            adsManager.CallEventInMainThread(delegate
            {
                if (adsConfig.Logging)
                    Debug.Log("[AdMobHandler]: HandleInterstitialOpened event received");

                adsManager.OnProviderAdDisplayed(providerType, AdType.Interstitial);
            });
        }

        public void HandleInterstitialClosed()
        {
            adsManager.CallEventInMainThread(delegate
            {
                if (adsConfig.Logging)
                    Debug.Log("[AdMobHandler]: HandleInterstitialClosed event received");

                adsManager.OnProviderAdClosed(providerType, AdType.Interstitial);

                adsManager.ExecuteInterstitialCallback(true);

                adsManager.ResetInterstitialDelayTime();
                adsManager.RequestInterstitial();
            });
        }

        private void HandleInterstitialClicked()
        {
            adsManager.CallEventInMainThread(delegate
            {
                if (adsConfig.Logging)
                    Debug.Log("[AdMobHandler]: HandleInterstitialClicked event received");
            });
        }
    #endregion

    #region RewardedVideo Callback
        private void HandleRewardBasedVideoFailedToShow(AdError error)
        {
            adsManager.CallEventInMainThread(delegate
            {
                adsManager.ExecuteRewardVideoCallback(false);

                if (adsConfig.Logging)
                    Debug.Log("[AdMobHandler]: HandleRewardBasedVideoFailedToShow event received with message: " + error);

                rewardedRetryAttempt++;
                float retryDelay = Mathf.Pow(2, rewardedRetryAttempt);

                DelayedCall(rewardedRetryAttempt, () => adsManager.RequestRewardBasedVideo(), true).Forget();
            });
        }

        public void HandleRewardBasedVideoOpened()
        {
            adsManager.CallEventInMainThread(delegate
            {
                if (adsConfig.Logging)
                    Debug.Log("[AdMobHandler]: HandleRewardBasedVideoOpened event received");
            });
        }

        public void HandleRewardBasedVideoClosed()
        {
            adsManager.CallEventInMainThread(delegate
            {
                if (adsConfig.Logging)
                    Debug.Log("[AdMobHandler]: HandleRewardBasedVideoClosed event received");

                adsManager.OnProviderAdClosed(providerType, AdType.RewardedVideo);
            });
        }

        private void HandleRewardBasedVideoClicked()
        {
            adsManager.CallEventInMainThread(delegate
            {
                if (adsConfig.Logging)
                    Debug.Log("[AdMobHandler]: HandleRewardBasedVideoClicked event received");
            });
        }
        #endregion

        //-----------------------------------------------------------------------------------------

        public string GetBannerID()
        {
#if UNITY_ANDROID
            return adsConfig.AdMobContainer.AndroidBannerID;
#elif UNITY_IOS
            return adsConfig.AdMobContainer.IOSBannerID;
#else
            return "unexpected_platform";
#endif
        }

        public string GetInterstitialID()
        {
#if UNITY_ANDROID
            return adsConfig.AdMobContainer.AndroidInterstitialID;
#elif UNITY_IOS
            return adsConfig.AdMobContainer.IOSInterstitialID;
#else
            return "unexpected_platform";
#endif
        }

        public string GetRewardedVideoID()
        {
#if UNITY_ANDROID
            return adsConfig.AdMobContainer.AndroidRewardedVideoID;
#elif UNITY_IOS
            return adsConfig.AdMobContainer.IOSRewardedVideoID;
#else
            return "unexpected_platform";
#endif
        }
    }
}
#endif