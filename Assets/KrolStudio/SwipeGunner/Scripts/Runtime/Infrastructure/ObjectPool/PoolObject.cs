using System.Collections.Generic;
using UnityEngine;

namespace KrolStudio
{
    public class PoolObject<T> : IPoolObject<T> where T : Component
    {
        readonly IObjectFactory<T> factory;
        readonly IPoolObjectsContainer poolObjectsContainer;

        readonly Stack<T> freeInstances;

        Transform poolContainer;
        
        public event System.Action<T> OnFreeCallback;

        public PoolObject(IObjectFactory<T> factory, int initialSize, IPoolObjectsContainer poolObjectsContainer, string poolName)
        {
            freeInstances = new Stack<T>(initialSize);

            this.factory = factory;
            this.poolObjectsContainer = poolObjectsContainer;

            poolContainer = poolObjectsContainer.CreatePoolContainer(poolName);

            SpawnObjects(initialSize);
        }

        private void SpawnObjects(int initialSize)
        {
            for (int i = 0; i < initialSize; ++i)
            {
                T obj = Create();
                if (obj == null) continue;
                obj.gameObject.SetActive(false);
                //obj.name = Time.realtimeSinceStartup.ToString();
                freeInstances.Push(obj);
            }
        }

        private T Create()
        {
            T createdObject = factory.Create();
            createdObject?.transform.SetParent(poolContainer);
            return createdObject;
        }

        public T Get(Transform parent = null)
        {
            T item = freeInstances.Count > 0 ? freeInstances.Pop() : Create();

            if(item == null) return null;

            if (item is IPoolReturnable<T> link)
                link.OnReturned += Free;

            item.transform.SetParent(parent);

            item.gameObject.SetActive(true);

            return item;
        }

        public T Get(Vector3 position, Transform parent = null)
        {
            T item = Get(parent);

            item.gameObject.transform.position = position;

            return item;
        }

        public T Get(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            T item = Get(parent);

            item.gameObject.transform.position = position;
            item.gameObject.transform.rotation = rotation;

            return item;
        }

        public void Clear()
        {
            int count = freeInstances.Count;

            for (int i = 0; i < count; i++)
            {
                Object.Destroy(freeInstances.Pop().gameObject);
            }
        }

        public void Free(T obj)
        {
            if(obj == null || obj.gameObject == null)
            {
                return;
            }

            if(obj.transform.parent != poolContainer)
                obj.transform.SetParent(poolContainer);

            if (obj is IPoolReturnable<T> link)
                link.OnReturned -= Free;

            obj.gameObject.SetActive(false);
           
            freeInstances.Push(obj);

            OnFreeCallback?.Invoke(obj);
        }
    }
}