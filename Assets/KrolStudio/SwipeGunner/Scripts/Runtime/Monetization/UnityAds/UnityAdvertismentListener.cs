#if MODULE_UNITYADS

using UnityEngine;
using UnityEngine.Advertisements;

namespace KrolStudio
{ 
    public class UnityAdvertismentListener : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener, IUnityAdsInitializationListener
    {
        private const int INIT_CHECK_MAX_ATTEMPT_AMOUNT = 5;

        private UnityAdsLegacyHandler adsHandler;
        private AdsConfig adsConfig;
        private AdsManager adsManager;

        public void Init(AdsConfig adsConfig, UnityAdsLegacyHandler adsHandler, AdsManager adsManager)
        {
            this.adsConfig = adsConfig;
            this.adsHandler = adsHandler;
            this.adsManager = adsManager;
        }

        public void OnInitializationComplete()
        {
            if (adsConfig.Logging)
                Debug.Log("Unity Ads is initialized!");
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            adsHandler.initializationAttemptCount++;

            if (adsConfig.Logging)
                Debug.Log("OnInitializationFailed event error:" + error.ToString() + "    message: " + message);

            if (adsHandler.initializationAttemptCount <= INIT_CHECK_MAX_ATTEMPT_AMOUNT)
            {
                Advertisement.Initialize(adsHandler.AppId, adsConfig.DebugMode, this);
            }
            else
            {
                if (adsConfig.Logging)
                    Debug.Log("OnInitializationFailed in every attempt");
            }
        }

        public void OnUnityAdsAdLoaded(string placementId)
        {
            if (placementId.Equals(adsHandler.PlacementBannerID))
            {
                Advertisement.Banner.Show(adsHandler.PlacementBannerID);

                if (adsConfig.Logging)
                    Debug.Log("OnUnityAdsAdLoaded - banner loaded");
            }
            else if (placementId.Equals(adsHandler.PlacementInterstitialID))
            {
                adsHandler.interstitialIsLoaded = true;

                if (adsConfig.Logging)
                    Debug.Log("OnUnityAdsAdLoaded - interstitial loaded");
            }
            else if (placementId.Equals(adsHandler.PlacementRewardedVideoID))
            {
                adsHandler.rewardVideoIsLoaded = true;

                if (adsConfig.Logging)
                    Debug.Log("OnUnityAdsAdLoaded - rewardVideo loaded");
            }
        }

        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        {
            if (placementId == adsHandler.PlacementInterstitialID)
            {
                adsHandler.interstitialIsLoaded = false;
            }
            else if (placementId == adsHandler.PlacementRewardedVideoID)
            {
                adsHandler.rewardVideoIsLoaded = false;
            }

            if (adsConfig.Logging)
                Debug.Log("OnUnityAdsFailedToLoad - " + placementId + ". Error - " + error + " .Message: " + message);
        }

        public void OnUnityAdsShowClick(string placementId)
        {
            if (adsConfig.Logging)
                Debug.Log("OnUnityAdsFailedToLoad - " + placementId);
        }

        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            if (placementId == adsHandler.PlacementInterstitialID)
            {
                adsHandler.interstitialIsLoaded = false;

                adsManager.OnProviderAdClosed(AdProvider.UnityAdsLegacy, AdType.Interstitial);
                adsManager.ExecuteInterstitialCallback(showCompletionState == UnityAdsShowCompletionState.COMPLETED);
                adsManager.ResetInterstitialDelayTime();
                adsManager.RequestInterstitial();
            }
            else if (placementId == adsHandler.PlacementRewardedVideoID)
            {
                adsHandler.rewardVideoIsLoaded = false;

                bool state = showCompletionState == UnityAdsShowCompletionState.COMPLETED;

                adsManager.ExecuteRewardVideoCallback(state);
                adsManager.OnProviderAdClosed(AdProvider.UnityAdsLegacy, AdType.RewardedVideo);
                adsManager.ResetInterstitialDelayTime();
                adsManager.RequestRewardBasedVideo();
            }

            if (adsConfig.Logging)
                Debug.Log("OnUnityAdsDidFinish - " + placementId + ". Result - " + showCompletionState);
        }

        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            if (adsConfig.Logging)
                Debug.Log("OnUnityAdsShowFailure - " + placementId + " - " + message);

            if (error == UnityAdsShowError.NOT_READY)
            {
                if (placementId == adsHandler.PlacementInterstitialID)
                {
                    adsManager.RequestInterstitial();
                }
                else if (placementId == adsHandler.PlacementRewardedVideoID)
                {
                    adsManager.RequestRewardBasedVideo();
                }
            }
        }

        public void OnUnityAdsShowStart(string placementId)
        {
            if (placementId == adsHandler.PlacementInterstitialID)
            {
                adsManager.OnProviderAdLoaded(AdProvider.UnityAdsLegacy, AdType.Interstitial);
            }
            else if (placementId == adsHandler.PlacementRewardedVideoID)
            {
                adsManager.OnProviderAdLoaded(AdProvider.UnityAdsLegacy, AdType.RewardedVideo);
            }

            if (adsConfig.Logging)
                Debug.Log("OnUnityAdsShowStart - " + placementId);
        }
    }
}
#endif