using System.Collections.Generic;
using UnityEngine;

namespace KrolStudio
{
    [System.Serializable]
    public class AdMobContainer
    {
        [SerializeField] string androidBannerID = "ca-app-pub-3940256099942544/6300978111";
        public string AndroidBannerID => androidBannerID;
        
        [SerializeField] string iOSBannerID = "ca-app-pub-3940256099942544/2934735716";
        public string IOSBannerID => iOSBannerID;

        // -----------------------------------------------------------------------------------------------------------------

        [SerializeField] string androidInterstitialID = "ca-app-pub-3940256099942544/1033173712";
        public string AndroidInterstitialID => androidInterstitialID;
       
        [SerializeField] string iOSInterstitialID = "ca-app-pub-3940256099942544/4411468910";
        public string IOSInterstitialID => iOSInterstitialID;

        // -----------------------------------------------------------------------------------------------------------------

        [SerializeField] string androidRewardedVideoID = "ca-app-pub-3940256099942544/5224354917";
        public string AndroidRewardedVideoID => androidRewardedVideoID;
        
        [SerializeField] string iOSRewardedVideoID = "ca-app-pub-3940256099942544/1712485313";
        public string IOSRewardedVideoID => iOSRewardedVideoID;

        // -----------------------------------------------------------------------------------------------------------------

        [SerializeField] BannerPlacementType bannerType = BannerPlacementType.Banner;
        public BannerPlacementType BannerType => bannerType;

        [SerializeField] BannerPosition bannerPosition = BannerPosition.Bottom;
        public BannerPosition BannerPosition => bannerPosition; 

        [SerializeField] List<string> testDevicesIDs;
        public List<string> TestDevicesIDs => testDevicesIDs;

        // -----------------------------------------------------------------------------------------------------------------

        public enum BannerPlacementType
        {
            Banner = 0,
            MediumRectangle = 1,
            IABBanner = 2,
            Leaderboard = 3,
        }
    }
}
