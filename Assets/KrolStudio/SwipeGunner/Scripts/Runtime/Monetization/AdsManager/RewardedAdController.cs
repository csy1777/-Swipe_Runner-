using System;
using UnityEngine;

namespace KrolStudio
{
    public class RewardedAdController
    {
        private AdsConfig config;
        private AdProviderRegistry registry;
        private AdProviderHandler.RewardedVideoCallback currentCallback;
        private bool waitingForCallback;

        public event Action<bool> RewardReceived;

        public void Initialize(AdsConfig config, AdProviderRegistry registry)
        {
            this.config = config;
            this.registry = registry;
        }

        public bool IsLoaded()
        {
            var handler = registry.GetHandler(config.RewardedVideoType);
            return handler != null &&
                   handler.IsInitialised() &&
                   handler.IsRewardedVideoLoaded();
        }

        public void Request()
        {
            var handler = registry.GetHandler(config.RewardedVideoType);

            if (handler == null ||
                !handler.IsInitialised() ||
                handler.IsRewardedVideoLoaded())
                return;

            handler.RequestRewardedVideo();
        }

        public void Show(AdProviderHandler.RewardedVideoCallback callback, bool showErrorMessage = true)
        {
            currentCallback = callback;
            waitingForCallback = true;

            var handler = registry.GetHandler(config.RewardedVideoType);

            if (handler == null ||
                !handler.IsInitialised() ||
                !handler.IsRewardedVideoLoaded())
            {
                ExecuteCallback(false);

                if (showErrorMessage)
                    Debug.LogError("Network error. Please try again later");

                return;
            }

            handler.ShowRewardedVideo(callback);
        }

        public void ExecuteCallback(bool result)
        {
            if (currentCallback != null && waitingForCallback)
            {
                currentCallback.Invoke(result);
                waitingForCallback = false;

                if (config.Logging)
                    Debug.Log($"[RewardedAdController]: Reward received: {result}");

                RewardReceived?.Invoke(result);
            }
        }
    }
}