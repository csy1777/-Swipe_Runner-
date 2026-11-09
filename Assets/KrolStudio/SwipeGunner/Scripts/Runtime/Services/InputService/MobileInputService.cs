using System;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class MobileInputService : IInputService, ITickable
    {
        private const float SwipeDeadZone = 1f;

        public event Action<float> OnSwipe;
        public event Action OnButtonDown;
        public event Action OnButtonUp;

        private Vector2 previousPosition;

        readonly PauseManager pauseManager;

        [Inject]
        public MobileInputService(PauseManager pauseManager)
        {
            this.pauseManager = pauseManager;
        }

        public void Tick()
        {
            if (pauseManager.IsPaused) return;

            if (Input.GetMouseButtonDown(0))
            {
                previousPosition = Input.mousePosition;
                OnButtonDown?.Invoke();
            }

            if (Input.GetMouseButton(0))
            {
                Vector2 currentPosition = Input.mousePosition;
                float deltaX = currentPosition.x - previousPosition.x;

                if (Mathf.Abs(deltaX) > SwipeDeadZone)
                {
                    OnSwipe?.Invoke(deltaX);
                }

                previousPosition = currentPosition;
            }

            if (Input.GetMouseButtonUp(0))
            {
                OnButtonUp?.Invoke();
            }
        }
    }
}