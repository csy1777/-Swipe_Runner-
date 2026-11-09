using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace KrolStudio
{
    public class OpenWindowButton : MonoBehaviour
    {
        [SerializeField] WindowType windowType;
        [SerializeField] Button button;
        //[SerializeField] bool returnPage;
        [SerializeField] bool overlayPage;

        IWindowService windowService;

        [Inject]
        void Construct(IWindowService windowService)
        {
            this.windowService = windowService;
        }

        void OnEnable()
        {
            button.onClick.AddListener(OnClick);
        }

        void OnDisable()
        {
            button.onClick.RemoveListener(OnClick);
        }

        void OnClick()
        {
            windowService.Open(windowType, overlayPage);

            //switch (windowType)
            //{
            //    case WindowType.SettingsWindow:
            //        windowService.Open(windowType, overlayPage);
            //        break;
            //}
        }
    }
}