using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class InteractorPoolContainer : IInteractorPoolContainer
    {
        Dictionary<Type, IPoolObject> container = new();

        readonly IPoolObjectsContainer objectsContainer;

        [Inject]
        public InteractorPoolContainer(IPoolObjectsContainer objectsContainer)
        {
            this.objectsContainer = objectsContainer;
        }

        public bool TryGet<T>(out T interactor) where T : IPoolObject<T>
        {
            if (container.TryGetValue(typeof(T), out IPoolObject value) && value is T typed)
            {
                interactor = typed;
                return true;
            }

            interactor = default;
            return false;
        }

        public IPoolObject<T> GetPool<T>() where T : Component
        {
            return container[typeof(IPoolObject<T>)] as PoolObject<T>;
        }

        public void CreatePool<T, T1>(T1 factory, int initialCount, string poolName) where T : Component where T1 : IObjectFactory<T>
        {
            container.Add(typeof(IPoolObject<T>), new PoolObject<T>(factory, initialCount, objectsContainer, poolName));
        }

        public void CleanUp()
        {
            container.Clear();
        }
    }
}