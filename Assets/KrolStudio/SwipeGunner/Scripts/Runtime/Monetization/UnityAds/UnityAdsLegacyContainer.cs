using UnityEngine;

namespace KrolStudio
{
    [System.Serializable]
    public class UnityAdsLegacyContainer
    {
        [Header("Application ID")]
        [SerializeField] string androidAppID = "1111";
        public string AndroidAppID => androidAppID;
        
        [SerializeField] string iOSAppID = "1111";
        public string IOSAppID => iOSAppID;

        // -----------------------------------------------------------------------------------------------------------------

        [Header("Banner ID")]
        [SerializeField] string androidBannerID = "Banner_Android";
        public string AndroidBannerID => androidBannerID;
        
        [SerializeField] string iOSBannerID = "Banner_iOS";
        public string IOSBannerID => iOSBannerID;

        // -----------------------------------------------------------------------------------------------------------------

        [Header("Interstitial ID")] 
        [SerializeField] string androidInterstitialID = "Interstitial_Android";
        public string AndroidInterstitialID => androidInterstitialID;
        
        [SerializeField] string iOSInterstitialID = "Interstitial_iOS";
        public string IOSInterstitialID => iOSInterstitialID;

        // -----------------------------------------------------------------------------------------------------------------

        [Header("Rewarded Video ID")]
        [SerializeField] string androidRewardedVideoID = "Rewarded_Android";
        public string AndroidRewardedVideoID => androidRewardedVideoID;
        
        [SerializeField] string iOSRewardedVideoID = "Rewarded_iOS";
        public string IOSRewardedVideoID => iOSRewardedVideoID;

        // -----------------------------------------------------------------------------------------------------------------

        [Space]
        [SerializeField] BannerPlacement bannerPosition = BannerPlacement.BOTTOM_CENTER;
        public BannerPlacement BannerPosition => bannerPosition;

        public enum BannerPlacement
        {
            TOP_LEFT = 0,
            TOP_CENTER = 1,
            TOP_RIGHT = 2,
            BOTTOM_LEFT = 3,
            BOTTOM_CENTER = 4,
            BOTTOM_RIGHT = 5,
            CENTER = 6
        }
    }
}
