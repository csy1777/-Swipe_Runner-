using UnityEngine;

namespace KrolStudio
{
    public abstract class AdProviderHandler
    {
        protected AdProvider providerType;
        public AdProvider ProviderType => providerType;

        protected AdsConfig adsConfig;
        protected AdsManager adsManager;

        protected bool isInitialised = false;

        public AdProviderHandler(AdProvider providerType)
        {
            this.providerType = providerType;
        }

        public bool IsInitialised()
        {
            return isInitialised;
        }

        protected void OnProviderInitialised()
        {
            isInitialised = true;

            adsManager.OnProviderInitialised(providerType);

            if (adsConfig.Logging)
                Debug.Log(string.Format("[AdsManager]: {0} is initialized!", providerType));
        }

        public abstract void Initialize(AdsConfig adsSettings, AdsManager adsManager);

        public abstract void ShowBanner();
        public abstract void HideBanner();
        public abstract void DestroyBanner();

        public abstract void RequestInterstitial();
        public abstract void ShowInterstitial(InterstitialCallback callback);
        public abstract bool IsInterstitialLoaded();

        public abstract void RequestRewardedVideo();
        public abstract void ShowRewardedVideo(RewardedVideoCallback callback);
        public abstract bool IsRewardedVideoLoaded();

        public virtual void SetGDPR(bool state) { }

        public delegate void RewardedVideoCallback(bool hasReward);
        public delegate void InterstitialCallback(bool isDisplayed);
    }
}