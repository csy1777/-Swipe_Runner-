using UnityEngine;

namespace KrolStudio
{
    [CreateAssetMenu(fileName = "AdsConfig", menuName = "Configs/AdsConfig")]
    public class AdsConfig : ScriptableObject
    {
        [field: SerializeField] public bool IsModuleActive { get; set; } = true;
        [field: SerializeField] public bool Logging { get; set; } = false;
        [field: SerializeField] public bool DebugMode { get; set; } = false;

        [Space]

        //-----------------------------------------------------------------------------------------

        [SerializeField] AdProvider bannerType = AdProvider.Disable;
        public AdProvider BannerType => bannerType;

        [SerializeField] AdProvider interstitialType = AdProvider.Disable;
        public AdProvider InterstitialType => interstitialType;

        [SerializeField] AdProvider rewardedVideoType = AdProvider.Disable;
        public AdProvider RewardedVideoType => rewardedVideoType;

        //-----------------------------------------------------------------------------------------

        // Providers
        [SerializeField] AdMobContainer adMobContainer;
        public AdMobContainer AdMobContainer => adMobContainer;

        [SerializeField] UnityAdsLegacyContainer unityAdsContainer;
        public UnityAdsLegacyContainer UnityAdsContainer => unityAdsContainer;

        [SerializeField] LevelPlayContainer levelPlayContainer;
        public LevelPlayContainer LevelPlayContainer => levelPlayContainer;

        //-----------------------------------------------------------------------------------------

        [SerializeField] bool loadAdsOnStart = true;
        public bool LoadAdsOnStart => loadAdsOnStart;

        [Space]
        [Tooltip("Delay in seconds before interstitial appearings on first game launch.")]
        [SerializeField] float interstitialFirstStartDelay = 40f;
        public float InterstitialFirstStartDelay => interstitialFirstStartDelay;

        [Tooltip("Delay in seconds before interstitial appearings.")]
        [SerializeField] float interstitialStartDelay = 40f;
        public float InterstitialStartDelay => interstitialStartDelay;

        [Tooltip("Delay in seconds between interstitial appearings.")]
        [SerializeField] float interstitialShowingDelay = 30f;
        public float InterstitialShowingDelay => interstitialShowingDelay;

        [SerializeField] bool autoShowInterstitial;
        public bool AutoShowInterstitial => autoShowInterstitial;

        //-----------------------------------------------------------------------------------------


        [SerializeField] ConsentDataContainer consentDataContainer;
        public ConsentDataContainer ConsentDataContainer => consentDataContainer;


        [Space]
        [SerializeField] bool isIDFAEnabled = false;
        public bool IsIDFAEnabled => isIDFAEnabled;

        [SerializeField] string trackingDescription = "Your data will be used to deliver personalized ads to you.";
        public string TrackingDescription => trackingDescription;

        [Space]
        [SerializeField] string privacyLink = "https://mywebsite.com/privacy";
        public string PrivacyLink => privacyLink;

        [SerializeField] string termsOfUseLink = "https://mywebsite.com/terms";
        public string TermsOfUseLink => termsOfUseLink;
    }

    public enum BannerPosition
    {
        Bottom = 0,
        Top = 1,
    }
}