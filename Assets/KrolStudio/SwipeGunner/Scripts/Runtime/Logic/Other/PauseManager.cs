using UnityEngine;
using System;

namespace KrolStudio
{
    public class PauseManager
    {
        public static event Action OnPauseStarted;
        public static event Action OnPauseEnded;
        
        public bool IsPaused { get; private set; } = false;

        public void Pause()
        {
            if (IsPaused) return;

            AudioListener.pause = true;

            Time.timeScale = 0f;
            IsPaused = true;
            OnPauseStarted?.Invoke();
        }

        public void Resume()
        {
            if (!IsPaused) return;

            AudioListener.pause = false;

            Time.timeScale = 1f;
            IsPaused = false;
            OnPauseEnded?.Invoke();
        }

        public void TogglePause()
        {
            if (IsPaused) Resume();
            else Pause();
        }
    }
}