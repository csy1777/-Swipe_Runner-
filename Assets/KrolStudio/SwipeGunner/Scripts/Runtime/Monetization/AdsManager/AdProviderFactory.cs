using System.Collections.Generic;
using System;

namespace KrolStudio
{
    public class AdProviderFactory
    {
        private readonly Dictionary<AdProvider, Func<AdProviderHandler>> factories = new()
        {
#if MODULE_ADMOB
            { AdProvider.AdMob, () => new AdMobHandler(AdProvider.AdMob) },
#endif
#if MODULE_UNITYADS
            { AdProvider.UnityAdsLegacy, () => new UnityAdsLegacyHandler(AdProvider.UnityAdsLegacy) },
#endif     
#if LEVELPLAY_DEPENDENCIES_INSTALLED      
            { AdProvider.LevelPlay, () => new LevelPlayHandler(AdProvider.LevelPlay) }
#endif   
        };

        public AdProviderHandler Create(AdProvider provider)
        {
            return factories.ContainsKey(provider) ? factories[provider]() : null;
        }

        public bool CanCreate(AdProvider provider)
        {
            return factories.ContainsKey(provider) && provider != AdProvider.Disable;
        }
    }
}