
namespace KrolStudio
{
    public class IDFAManager
    {
        private AdsConfig config;

        public void Initialize(AdsConfig config)
        {
            this.config = config;

#if UNITY_IOS
        if (config.IsIDFAEnabled)
        {
            RequestIDFAIfNeeded().Forget();
        }
#endif
        }

#if UNITY_IOS
    private async UniTaskVoid RequestIDFAIfNeeded()
    {
        await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

        if (!IsIDFADetermined())
        {
            if (config.Logging)
                Debug.Log("[IDFAManager]: Requesting IDFA..");

            ATTrackingStatusBinding.RequestAuthorizationTracking();
        }
    }
#endif

        public bool IsIDFADetermined()
        {
#if UNITY_IOS
        if (config.IsIDFAEnabled)
        {
            return ATTrackingStatusBinding.GetAuthorizationTrackingStatus() !=
                   ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED;
        }
#endif
            return true;
        }
    }
}