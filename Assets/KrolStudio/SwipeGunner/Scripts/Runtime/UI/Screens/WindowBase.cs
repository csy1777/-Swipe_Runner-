using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace KrolStudio
{
    public abstract class WindowBase : MonoBehaviour
    {
        [SerializeField] protected Button CloseButton;

        protected IWindowService WindowService;

        [Inject]
        public void Construct(IWindowService windowService)
        {
            WindowService = windowService;
        }

        protected virtual void CloseWindow()
        {
            CloseButton.interactable = false;
            WindowService.Close();
        }

        protected virtual void OnEnable()
        {
            CloseButton.interactable = true;
            CloseButton.onClick.AddListener(CloseWindow);

            Initialize();
            SubscribeUpdates();
        }

        protected virtual void OnDisable()
        {
            CloseButton.onClick.RemoveListener(CloseWindow);
            Cleanup();
        }

        protected abstract void Initialize();       // Used to initialize Unity components (with injected data)
        protected abstract void SubscribeUpdates();
        protected abstract void Cleanup();
    }
}