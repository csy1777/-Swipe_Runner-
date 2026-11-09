using System.Collections.Generic;
using System;
using UnityEngine;

namespace KrolStudio
{
    public class AdProviderRegistry
    {
        private Dictionary<AdProvider, AdProviderHandler> handlers = new();
        private AdsConfig config;

        public event Action<AdProvider> ProviderInitialised;
        public event Action<AdProvider, AdType> AdLoaded;
        public event Action<AdProvider, AdType> AdDisplayed;
        public event Action<AdProvider, AdType> AdClosed;

        public void Initialize(AdsConfig config, AdProviderFactory factory)
        {
            this.config = config;
            handlers.Clear();

            var providers = new[] { config.BannerType, config.InterstitialType, config.RewardedVideoType };

            foreach (var provider in providers)
            {
                if (provider != AdProvider.Disable && factory.CanCreate(provider) && !handlers.ContainsKey(provider))
                {
                    handlers[provider] = factory.Create(provider);
                }
            }
        }

        public void InitializeHandlers(AdsConfig config, AdsManager manager)
        {
            foreach (var handler in handlers.Values)
            {
                if (!handler.IsInitialised())
                {
                    if (config.Logging)
                        Debug.Log($"[AdProviderRegistry]: Initializing {handler.ProviderType}");

                    handler.Initialize(config, manager);
                }
            }
        }

        public AdProviderHandler GetHandler(AdProvider provider)
        {
            return handlers.ContainsKey(provider) ? handlers[provider] : null;
        }

        public bool HasHandler(AdProvider provider)
        {
            return handlers.ContainsKey(provider);
        }

        public bool IsInitialised(AdProvider provider)
        {
            var handler = GetHandler(provider);
            return handler != null && handler.IsInitialised();
        }

        public void OnProviderInitialised(AdProvider provider)
        {
            ProviderInitialised?.Invoke(provider);
        }

        public void OnAdLoaded(AdProvider provider, AdType adType)
        {
            AdLoaded?.Invoke(provider, adType);
        }

        public void OnAdDisplayed(AdProvider provider, AdType adType)
        {
            AdDisplayed?.Invoke(provider, adType);
        }

        public void OnAdClosed(AdProvider provider, AdType adType)
        {
            AdClosed?.Invoke(provider, adType);
        }
    }
}