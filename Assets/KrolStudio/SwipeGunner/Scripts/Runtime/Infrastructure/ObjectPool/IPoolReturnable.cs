using System;
using UnityEngine;

namespace KrolStudio
{
    public interface IPoolReturnable
    {
        void ReturnToPool();
    }

    public interface IPoolReturnable<T> : IPoolReturnable where T : Component
    {
        event Action<T> OnReturned;
    }
}