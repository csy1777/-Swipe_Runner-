using UnityEngine;

namespace KrolStudio
{
    [System.Serializable]
    public class LevelPlayContainer
    {
        [Header("Application ID")]
        [SerializeField] string androidAppKey = "85460dcd";
        public string AndroidAppKey => androidAppKey;

        [SerializeField] string iOSAppKey = "8545d445";
        public string IOSAppKey => iOSAppKey;

        // -----------------------------------------------------------------------------------------------------------------

        [SerializeField] string androidBannerID = "thnfvcsog13bhn08";
        public string AndroidBannerID => androidBannerID;

        [SerializeField] string iOSBannerID = "iep3rxsyp9na3rw8";
        public string IOSBannerID => iOSBannerID;

        // -----------------------------------------------------------------------------------------------------------------

        [SerializeField] string androidInterstitialID = "aeyqi3vqlv6o8sh9";
        public string AndroidInterstitialID => androidInterstitialID;

        [SerializeField] string iOSInterstitialID = "wmgt0712uuux8ju4";
        public string IOSInterstitialID => iOSInterstitialID;

        // -----------------------------------------------------------------------------------------------------------------

        [SerializeField] string androidRewardedVideoID = "aeyqi3vqlv6o8sh9";
        public string AndroidRewardedVideoID => androidRewardedVideoID;

        [SerializeField] string iOSRewardedVideoID = "aeyqi3vqlv6o8sh9";
        public string IOSRewardedVideoID => iOSRewardedVideoID;

        // -----------------------------------------------------------------------------------------------------------------

        [SerializeField] BannerPosition bannerPosition = BannerPosition.Bottom;
        public BannerPosition BannerPosition => bannerPosition;
    }
}