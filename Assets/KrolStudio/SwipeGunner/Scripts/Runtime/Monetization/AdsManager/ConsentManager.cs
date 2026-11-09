using GoogleMobileAds.Ump.Api;
using System;
using UnityEngine;

namespace KrolStudio
{
    public class ConsentManager
    {
        private ConsentState consentState = ConsentState.Unknown;
        private AdsConfig config;

        public enum ConsentState
        {
            Unknown,
            Obtained,
            NotObtained
        }

        public ConsentState CurrentState => consentState;
        public bool CanShowPersonalizedAds => consentState == ConsentState.Obtained;

        public event Action<ConsentState> ConsentStateChanged;

        public void Initialize(AdsConfig config)
        {
            this.config = config;

            if (!config.ConsentDataContainer.IsUMPEnabled)
                return;

            RequestConsent();
        }

        private void RequestConsent()
        {
            var consentSettings = config.ConsentDataContainer;

            if (consentSettings.DebugMode)
            {
                ConsentInformation.Reset();
            }

            ConsentRequestParameters request = CreateConsentRequest(consentSettings);
            ConsentInformation.Update(request, OnConsentInfoUpdated);
        }

        private ConsentRequestParameters CreateConsentRequest(ConsentDataContainer settings)
        {
            if (settings.DebugMode)
            {
                return new ConsentRequestParameters
                {
                    TagForUnderAgeOfConsent = false,
                    ConsentDebugSettings = new ConsentDebugSettings
                    {
                        DebugGeography = settings.DebugGeography,
                        TestDeviceHashedIds = settings.TestDeviceHashedIds
                    }
                };
            }

            return new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false
            };
        }

        private void OnConsentInfoUpdated(FormError error)
        {
            if (error != null)
            {
                Debug.LogError("[ConsentManager]: Consent error: " + error.Message);
                UpdateConsentState(ConsentState.NotObtained);
                return;
            }

            if (ConsentInformation.IsConsentFormAvailable())
            {
                ConsentForm.LoadAndShowConsentFormIfRequired(OnConsentFormResult);
            }
            else
            {
                UpdateConsentStateFromInfo();
            }
        }

        private void OnConsentFormResult(FormError formError)
        {
            if (formError != null)
            {
                Debug.LogError("[ConsentManager]: Consent form error: " + formError.Message);
                UpdateConsentState(ConsentState.NotObtained);
                return;
            }

            UpdateConsentStateFromInfo();
        }

        private void UpdateConsentStateFromInfo()
        {
            var newState = ConsentInformation.ConsentStatus == ConsentStatus.Obtained
                ? ConsentState.Obtained
                : ConsentState.NotObtained;

            UpdateConsentState(newState);
        }

        private void UpdateConsentState(ConsentState newState)
        {
            consentState = newState;

            if (config.Logging)
                Debug.Log("[ConsentManager]: Consent state = " + consentState);

            ConsentStateChanged?.Invoke(consentState);
        }
    }
}
