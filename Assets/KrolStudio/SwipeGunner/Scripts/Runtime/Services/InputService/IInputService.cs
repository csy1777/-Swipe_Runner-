using System;

namespace KrolStudio
{
    public interface IInputService
    {
        event Action<float> OnSwipe;
        event Action OnButtonDown;
        event Action OnButtonUp;
    }
}