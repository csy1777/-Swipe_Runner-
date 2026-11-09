using UnityEngine;

namespace KrolStudio
{
    public interface IPoolObjectsContainer
    {
        Transform CreatePoolContainer(string poolName);
    }
}