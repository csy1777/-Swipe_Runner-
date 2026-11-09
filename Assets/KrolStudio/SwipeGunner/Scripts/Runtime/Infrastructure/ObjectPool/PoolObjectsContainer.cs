using UnityEngine;

namespace KrolStudio
{
    public class PoolObjectsContainer : IPoolObjectsContainer
    {
        const string ObjectPoolsName = "RootPoolObjects";

        Transform objectPoolsParent;

        public PoolObjectsContainer()
        {
            objectPoolsParent = new GameObject(ObjectPoolsName).transform;
        }

        public Transform CreatePoolContainer(string poolName)
        {
            Transform poolContainer = new GameObject(poolName).transform;
            poolContainer.SetParent(objectPoolsParent);
            return poolContainer;
        }
    }
}
