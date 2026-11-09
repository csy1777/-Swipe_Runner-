using System;

namespace KrolStudio
{
    public class EnemyKillRewardText : PopUpText, IPoolReturnable<EnemyKillRewardText>
    {
        public event Action<EnemyKillRewardText> OnReturned;

        void Awake()
        {
            // If the object exists only in one scene -> no need to unsubscribe.
            // If using cross-scene pooling or DontDestroyOnLoad -> make sure to unsubscribe.

            OnFinalize += ReturnToPool;
        }

        public void ReturnToPool()
        {
            OnReturned?.Invoke(this);
        }
    }
}