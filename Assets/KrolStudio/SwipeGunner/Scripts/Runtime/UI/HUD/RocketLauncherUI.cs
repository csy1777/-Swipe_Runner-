using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace KrolStudio
{
    public class RocketLauncherUI : MonoBehaviour
    {
        [SerializeField] Button launchButton;
        [SerializeField] GameObject reloadHolder;
        [SerializeField] TextMeshProUGUI reloadText;
        [SerializeField] PopUpText rocketLaunchFailurePrefab;

        private int timer, currentTimer;
        private CtsHelper ctsHelper = new();

        public event Func<bool> OnRocketLaunch;
        
        public PopUpText LaunchFailurePrefab => rocketLaunchFailurePrefab;

        [Inject]
        void Construct(RocketConfig rocketConfig)
        {
            timer = rocketConfig.baseReload;
        }

        void OnEnable()
        {
            launchButton.onClick.AddListener(OnClick);
        }

        void OnDisable()
        {
            launchButton.onClick.RemoveListener(OnClick);
        }

#if UNITY_EDITOR
        void Update()
        {
            if(!IsReloading() && Input.GetKeyDown(KeyCode.F))
            {
                OnClick();
            }
        }
#endif

        public bool IsReloading() => currentTimer > 0;

        private async UniTask Reload()
        {
            try
            {
                currentTimer = timer;
                reloadText.text = currentTimer.ToString();

                launchButton.interactable = false;
                reloadHolder.SetActive(true);

                while (currentTimer > 0)
                {
                    currentTimer--;
                    
                    reloadText.text = currentTimer.ToString();
                    
                    await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: ctsHelper.Token);
                }

                launchButton.interactable = true;
                reloadHolder.SetActive(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (MissingReferenceException ex)
            {
                Debug.LogWarning($"The object was destroyed during animation: {ex.Message}", this);
            }
            catch (NullReferenceException ex)
            {
                Debug.LogWarning($"Missing link during animation: {ex.Message}", this);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error: {ex}", this);
            }
        }

        private void OnDestroy()
        {
            ctsHelper.Dispose();
        }

        void OnClick()
        {
            if(OnRocketLaunch != null && OnRocketLaunch.Invoke())
            {
                Reload().Forget();
            }
        }

        public void DisplayRocketLauncher(bool value)
        {
            gameObject.SetActive(value);
        }

        public void SubscribeToRocketLaunch(Func<bool> handler)
        {
            OnRocketLaunch += handler;
        }

        public void UnsubscribeFromRocketLaunch(Func<bool> handler)
        {
            OnRocketLaunch -= handler;
        }
    }
}
