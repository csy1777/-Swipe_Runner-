using System;
using UnityEngine;

namespace KrolStudio
{
    public class InterstitialAdController
    {
        private AdsConfig config;
        private AdProviderRegistry registry;
        private IPersistentProgressService progressService;
        private double lastInterstitialTime;
        private AdProviderHandler.InterstitialCallback currentCallback;

        public Action InterstitialConditions;
        public event Action<bool> InterstitialCompleted;

        public void Initialize(AdsConfig config, AdProviderRegistry registry, IPersistentProgressService progressService)
        {
            this.config = config;
            this.registry = registry;
            this.progressService = progressService;

            if (!PlayerPrefs.HasKey("FIRST_LAUNCH"))
            {
                lastInterstitialTime = Time.time + config.InterstitialFirstStartDelay;
                PlayerPrefs.SetInt("FIRST_LAUNCH", 1);
            }
            else
            {
                lastInterstitialTime = Time.time + config.InterstitialStartDelay;
            }
        }

        public bool IsLoaded()
        {
            var handler = registry.GetHandler(config.InterstitialType);
            return handler != null &&
                   progressService.Progress.IsForcedAdEnabled &&
                   handler.IsInterstitialLoaded();
        }

        public void Request()
        {
            var handler = registry.GetHandler(config.InterstitialType);

            if (handler == null ||
                !progressService.Progress.IsForcedAdEnabled ||
                !handler.IsInitialised() ||
                handler.IsInterstitialLoaded())
                return;

            handler.RequestInterstitial();
        }

        public void Show(AdProviderHandler.InterstitialCallback callback, bool ignoreConditions = false)
        {
            currentCallback = callback;

            var handler = registry.GetHandler(config.InterstitialType);

            if (handler == null ||
                !progressService.Progress.IsForcedAdEnabled ||
                (!ignoreConditions && (!CheckTime() || !CheckExtraConditions())) ||
                !handler.IsInitialised() ||
                !handler.IsInterstitialLoaded())
            {
                ExecuteCallback(false);
                return;
            }

            handler.ShowInterstitial(callback);
        }

        public void ExecuteCallback(bool result)
        {
            currentCallback?.Invoke(result);
            InterstitialCompleted?.Invoke(result);
        }

        public void ResetDelayTime()
        {
            lastInterstitialTime = Time.time + config.InterstitialShowingDelay;
        }

        public void SetDelayTime(float time)
        {
            lastInterstitialTime = Time.time + time;
        }

        public bool CheckTime()
        {
            if (config.Logging)
                Debug.Log($"[InterstitialAdController]: Time: {lastInterstitialTime} < {Time.time}");

            return lastInterstitialTime < Time.time;
        }

        private bool CheckExtraConditions()
        {
            if (InterstitialConditions == null)
                return true;

            bool state = true;
            var delegates = InterstitialConditions.GetInvocationList();

            foreach (var del in delegates)
            {
                if (!(bool)del.DynamicInvoke())
                {
                    state = false;
                    break;
                }
            }

            if (config.Logging)
                Debug.Log($"[InterstitialAdController]: Extra conditions: {state}");

            return state;
        }
    }
}