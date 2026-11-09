using TMPro;
using UnityEngine;

namespace KrolStudio
{
    public class HUDView : MonoBehaviour, IHudView, IRocketHud,IOverHeatHudView
    {
        [SerializeField] RocketLauncherUI rocketLauncherUI;
        [SerializeField] OverHeatUI overHeatUI;
        [SerializeField] MeterProgress meterProgress;
        [SerializeField] TextMeshProUGUI levelIndicator;

        [SerializeField] PopUpText outOfBulletsPrefab;
        [SerializeField] PopUpText itemMaxLevelPrefab;
        [SerializeField] PopUpText incomeUpPrefab;

        Object incomeUpMessage, maxLevelMessage, launchFailureMessage;

        void Start()
        {
            gameObject.SetActive(true);
            meterProgress.gameObject.SetActive(false);
        }

        public void OnEnable()
        {
            meterProgress.SetProgressView(0f);
        }


        public void ShowIncomeUpMessage()
        {
            if(incomeUpMessage == null)
            {
                incomeUpMessage = ShowPopUpMessage(incomeUpPrefab);
            }
        }

        public void ShowItemMaxLevelMessage()
        {
            if (maxLevelMessage == null)
            {
                maxLevelMessage = ShowPopUpMessage(itemMaxLevelPrefab);
            }
        }

        public void ShowOutOfBulletsMessage()
        {
            ShowPopUpMessage(outOfBulletsPrefab);
        }

        Object ShowPopUpMessage(PopUpText prefab)
        {
            PopUpText message = Instantiate(prefab, transform);
            message.OnFinalize += () => Destroy(message.gameObject);
            message.Play();
            return message;
        }

        public void UpdateMeterProgress(MovementTracker tracker)
        {
            meterProgress.SetTraveledDistance(Mathf.RoundToInt(tracker.Distance()));
            meterProgress.SetProgressView(tracker.Progress);
        }

        public void DisplayMeterProgress(bool value)
        {
            if (this == null || transform == null || meterProgress == null)
                return;
            meterProgress.gameObject.SetActive(value);
        }

        public void UpdateLevelIndicator(int level)
        {
            levelIndicator.text = $"LEVEL {level}";
        }

        public bool IsReloading()
        {
            return rocketLauncherUI.IsReloading();
        }

        public void DisplayRocketLauncher(bool value)
        {
            if (this == null || transform == null || rocketLauncherUI == null)
                return;
            rocketLauncherUI.DisplayRocketLauncher(value);
        }

        public void DisplayOverHeat(bool value)
        {
            if (this == null || transform == null || overHeatUI == null)
                return;
            overHeatUI.DisplayOverHeat(value);
        }

        public void ShowRocketLaunchFailureMessage()
        {
            if(launchFailureMessage == null)
            {
                launchFailureMessage = ShowPopUpMessage(rocketLauncherUI.LaunchFailurePrefab);
            }
        }

        public void SubscribeToRocketLaunch(System.Func<bool> handler)
        {
            rocketLauncherUI.SubscribeToRocketLaunch(handler);
        }

        public void UnsubscribeFromRocketLaunch(System.Func<bool> handler)
        {
            rocketLauncherUI.UnsubscribeFromRocketLaunch(handler);
        }
    }
}