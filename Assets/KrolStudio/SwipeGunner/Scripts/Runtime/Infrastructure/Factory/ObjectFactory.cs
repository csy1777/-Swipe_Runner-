using System;

namespace KrolStudio
{
    public class ObjectFactory<T> : IObjectFactory<T> where T : class
    {
        private readonly IGameFactory gameFactory;

        public ObjectFactory(IGameFactory gameFactory)
        {
            this.gameFactory = gameFactory;
        }

        public T Create()
        {
            if (typeof(T) == typeof(Tentacle))
            {
                return gameFactory.CreateTentacle() as T;
            }
            else if (typeof(T) == typeof(Enemy))
            {
                return gameFactory.CreateEnemy() as T;
            }
            else if (typeof(T) == typeof(DamagePopup))
            {
                return gameFactory.CreateDamagePopup() as T;
            }
            else if (typeof(T) == typeof(EnemyKillRewardText))
            {
                return gameFactory.CreateEnemyKillRewardText() as T;
            }
            else if (typeof(T) == typeof(Projectile))
            {
                return gameFactory.CreateProjectile() as T;
            }

            throw new InvalidOperationException($"Factory not supported for type {typeof(T)}");
        }
    }
}