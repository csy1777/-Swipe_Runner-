using System.Collections.Generic;
using Zenject;

namespace KrolStudio
{
    public class WindowService : IWindowService
    {
        readonly IWindowFactory windowFactory;

        Stack<WindowType> previousPages = new Stack<WindowType>();
        Dictionary<WindowType, WindowBase> createdWindows = new();

        WindowType currentWindowType;
        WindowBase currentWindow;
        
        [Inject]
        public WindowService(IWindowFactory windowFactory)
        {
            this.windowFactory = windowFactory;
        }

        public WindowBase GetWindow(WindowType windowType)
        {
            bool isCreatedWindow = createdWindows.TryGetValue(windowType, out WindowBase createdWindow);

            if (!isCreatedWindow)
            {
                createdWindow = CreateWindow(windowType);

                createdWindow.gameObject.SetActive(false);

                createdWindows.Add(windowType, createdWindow);
            }

            return createdWindow;
        }

        public void Open(WindowType windowType, bool overlayPage = false)
        {
            SetWindow(windowType, overlayPage);

            if (!TryShowCreatedWindow(windowType))
            {
                currentWindow = CreateWindow(windowType);
                createdWindows.Add(windowType, currentWindow);
            }
        }

        private WindowBase CreateWindow(WindowType windowType)
        {
            return windowType switch
            {
                WindowType.LaunchScreen => windowFactory.CreateWindow<LaunchScreen>(windowType),
                WindowType.TutorialScreen => windowFactory.CreateWindow<TutorialScreen>(windowType),
                WindowType.SettingsWindow => windowFactory.CreateWindow<SettingsWindow>(windowType),
                WindowType.CompleteScreen => windowFactory.CreateWindow<CompleteWindow>(windowType),
                WindowType.FailScreen => windowFactory.CreateWindow<FailWindow>(windowType),
                WindowType.FireModeScreen => windowFactory.CreateWindow<FireModeWindow>(windowType),
                _ => null
            };
        }

        private bool TryShowCreatedWindow(WindowType windowType)
        {
            bool isCreatedWindow = createdWindows.TryGetValue(windowType, out WindowBase windowObject);

            if (isCreatedWindow)
            {
                windowObject.gameObject.SetActive(true);
                currentWindow = windowObject;
            }

            return isCreatedWindow;
        }

        private void SetWindow(WindowType windowType, bool overlayPage = false)
        {
            if (currentWindow != null)
            {
                previousPages.Push(currentWindowType);

                if(!overlayPage)
                    DisableWindow();
            }

            currentWindowType = windowType;
        }

        private void DisableWindow()
        {
            currentWindow.gameObject.SetActive(false);
            currentWindow = null;
        }

        public void Close()
        {
            if (currentWindow == null)
                return;

            DisableWindow();

            if (previousPages.Count > 0)
            {
                WindowType windowType = previousPages.Pop();
                Open(windowType);
            }
        }
    }
}