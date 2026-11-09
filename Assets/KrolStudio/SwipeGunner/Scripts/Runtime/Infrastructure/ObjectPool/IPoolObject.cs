using UnityEngine;

namespace KrolStudio
{
    public interface IPoolObject
    {
        void Clear();
    }

    public interface IPoolObject<T> : IPoolObject
    {
        T Get(Transform parent = null);
        T Get(Vector3 position, Transform parent = null);
        T Get(Vector3 position, Quaternion rotation, Transform parent = null);

        void Free(T obj);
    }
}

