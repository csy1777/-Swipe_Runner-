using UnityEngine;

namespace KrolStudio
{
    public class LogService : ILogService
    {
        public void Log(string message)
        {
#if UNITY_EDITOR
            Debug.Log(message);
#endif
        }

        public void LogError(string message)
        {
#if UNITY_EDITOR
            Debug.LogError(message);
#endif
        }

        public void LogWarning(string message)
        {
#if UNITY_EDITOR
            Debug.LogWarning(message);
#endif
        }
    }
}