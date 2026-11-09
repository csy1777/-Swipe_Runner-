using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using Zenject;

namespace KrolStudio
{
    public class EnemyPlacer
    {
        Dictionary<EnemySpawnPoint, List<IPoolReturnable>> enemiesByPoint = new();

        IInteractorPoolContainer interactorPool;
        IPlayerContext playerContext;

        int pointerStart, pointerEnd;

        Level currentLevel;

        [Inject]
        void Construct(IInteractorPoolContainer interactorPool, IPlayerContext playerContext)
        {
            this.interactorPool = interactorPool;
            this.playerContext = playerContext;
        }

        public void Initialize(Level currentLevel)
        {
            this.currentLevel = currentLevel;
        }

        void SpawnEnemy(EnemySpawnPoint spawn)
        {
            enemiesByPoint.Add(spawn, new List<IPoolReturnable>());

            float3 worldPos = default, worldTangent, worldUp;

            if (spawn.zoneType == ZoneType.RectAlong)
            {
                if (spawn.enableSpawnGradient)
                {
                    int i, j;

                    if (spawn.colorKeys.Length > 0)
                    {
                        for (i = 0; i < spawn.colorKeys[0].enemyCount; i++)
                        {
                            Spawn(UnityEngine.Random.Range(spawn.startRect, Mathf.Lerp(spawn.startRect, spawn.endRect, spawn.colorKeys[0].time)));
                        }
                    }

                    for (i = 0; i < spawn.colorKeys.Length - 1; i++)
                    {
                        for (j = 0; j < spawn.colorKeys[i + 1].enemyCount; j++)
                        {
                            Spawn(UnityEngine.Random.Range(Mathf.Lerp(spawn.startRect, spawn.endRect, spawn.colorKeys[i].time),
                                                           Mathf.Lerp(spawn.startRect, spawn.endRect, spawn.colorKeys[i + 1].time)));
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < spawn.enemyCount; i++)
                    {
                        Spawn(UnityEngine.Random.Range(spawn.startRect, spawn.endRect));
                    }
                }
            }
            else
            {
                for (int i = 0; i < spawn.enemyCount; i++)
                {
                    Spawn(spawn.position);
                }
            }

            void Spawn(float position)
            {
                CalculateOrientation(position, out worldPos, out worldTangent, out worldUp);
                Quaternion rotation = Quaternion.LookRotation(worldTangent, worldUp);

                IPoolReturnable newEnemy = SpawnMob(spawn.enemyType, worldPos, rotation, spawn);

                enemiesByPoint[spawn].Add(newEnemy);
            }
        }

        private void CalculateOrientation(float t, out float3 worldPos, out float3 worldTangent, out float3 worldUp)
        {
            SplineUtility.Evaluate(currentLevel.playerPathSpline.Spline, t, out worldPos, out worldTangent, out worldUp);

            worldPos = currentLevel.playerPathSpline.transform.TransformPoint(worldPos);
            worldTangent = currentLevel.playerPathSpline.transform.TransformDirection(worldTangent);
            worldUp = currentLevel.playerPathSpline.transform.TransformDirection(worldUp);
        }

        private IPoolReturnable SpawnMob(EnemyType enemyType, Vector3 worldPos, Quaternion rotation, EnemySpawnPoint spawn)
        {
            return enemyType switch
            {
                EnemyType.Stickman => SpawnMob<Enemy>(worldPos, rotation, spawn),
                EnemyType.Tentacle => SpawnMob<Tentacle>(worldPos, rotation, spawn),
                _ => default
            };
        }

        T SpawnMob<T>(Vector3 worldPos, Quaternion rotation, EnemySpawnPoint spawn) where T : MonoBehaviour
        {
            T newMob = interactorPool.GetPool<T>().Get();
            newMob.transform.rotation = rotation;

            if (newMob is Enemy enemy)
            {
                enemy.SpawnPosition = worldPos;
            }
            //else if (newMob is Tentacle tentacle)
            //{
            //}

            if (spawn.zoneType == ZoneType.Circle)
            {
                newMob.transform.position = GetRandomPointInCircle(worldPos, rotation, spawn.circleRadius);
            }
            else if (spawn.zoneType == ZoneType.Rect)
            {
                newMob.transform.position = GetRandomPositionInRect(worldPos, rotation, spawn.rectSize);
            }
            else if(spawn.zoneType == ZoneType.RectAlong)
            {
                newMob.transform.position = GetRandomPositionInRectAlong(worldPos, rotation, spawn.rectWidth);
            }

            return newMob;
        }

        private Vector3 GetRandomPositionInRectAlong(Vector3 position, Quaternion rotation, float rectWidth)
        {
            Vector3 halfWidth = rotation * new Vector3(rectWidth * 0.5f, 0f, 0f);
            return Vector3.Lerp(position + halfWidth, position - halfWidth, UnityEngine.Random.value);
        }
 
        private Vector3 GetRandomPointInCircle(Vector3 center, Quaternion rotation, float radius)
        {
            Vector2 randomPoint2D = UnityEngine.Random.insideUnitCircle * radius;
            return center + rotation * new Vector3(randomPoint2D.x, 0f, randomPoint2D.y);
        }

        private Vector3 GetRandomPositionInRect(Vector3 center, Quaternion rotation, Vector2 size)
        {
            var randomX = UnityEngine.Random.Range(-size.x * 0.5f, size.x * 0.5f);
            var randomZ = UnityEngine.Random.Range(-size.y * 0.5f, size.y * 0.5f);
            return center + rotation * new Vector3(randomX, 0f, randomZ);
        }

        public void DestroyEnemy(Enemy enemy)
        {
            foreach (var item in enemiesByPoint)
            {
                if(item.Value.Contains(enemy))
                {
                    enemy.ReturnToPool();
                    item.Value.Remove(enemy);
                    return;
                }
            }
        }

        public void DestroyEnemies(EnemySpawnPoint spawn)
        {
            if (!enemiesByPoint.TryGetValue(spawn, out List<IPoolReturnable> enemyList)) return;

            foreach (IPoolReturnable enemy in enemyList)
                enemy.ReturnToPool();

            enemyList.Clear();
            enemiesByPoint.Remove(spawn);
        }

        public void Tick()
        {
            foreach (var spawn in currentLevel.enemySpawnPoints)
            {
                if (!spawn.hasSpawned && playerContext.SplineMover.NormalizedTime >= spawn.SpawnTrigger)
                {
                    spawn.hasSpawned = true;
                    SpawnEnemy(spawn);
                }

                if (!spawn.hasDespawned && playerContext.SplineMover.NormalizedTime >= spawn.DeSpawnTrigger)
                {
                    spawn.hasDespawned = true;
                    DestroyEnemies(spawn);
                }
            }
        }
    }
}