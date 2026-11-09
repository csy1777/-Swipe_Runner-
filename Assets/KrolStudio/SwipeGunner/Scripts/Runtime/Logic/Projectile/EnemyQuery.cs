using System.Collections.Generic;
using UnityEngine;

namespace KrolStudio
{
    /// <summary>
    /// 敌人范围查询工具。
    ///
    /// 从原来的 ImpactExplosion 里抽出来的：那里"查范围 + 扣血 + 播特效"三件事混在一起，
    /// 导致燃烧只能白蹭爆炸的范围。现在范围只负责"找出附近有哪些敌人"，
    /// 给谁上什么效果由 BulletEffects 决定。
    ///
    /// 注意：
    ///   - 这里【不】创建/销毁刚体，也不改物理状态，只在物理查询层读数据；
    ///   - 缓冲区是静态复用的，避免每次命中都产生 GC；因此本方法不可重入
    ///     （当前调用链是单线程顺序执行，安全）。
    /// </summary>
    public static class EnemyQuery
    {
        /// <summary>范围查询的缓冲区大小。同屏敌人超过这个数量时，多出来的会被忽略。</summary>
        const int BufferSize = 32;

        static readonly Collider[] Buffer = new Collider[BufferSize];

        /// <summary>缓存 "Enemy" 层的 mask，避免每次命中都调用 LayerMask.GetMask（内部要做字符串查找）。</summary>
        static int enemyMask = -1;
        static int EnemyMask
        {
            get
            {
                if (enemyMask < 0)
                    enemyMask = LayerMask.GetMask("Enemy");

                return enemyMask;
            }
        }

        /// <summary>
        /// 找出以 <paramref name="center"/> 为球心、<paramref name="radius"/> 为半径内的所有存活敌人。
        /// 结果会去重（同一个敌人可能挂了多个碰撞体，例如布娃娃骨头）。
        /// </summary>
        /// <param name="center">查询中心（通常是子弹命中点）。</param>
        /// <param name="radius">半径（米）。&lt;= 0 时直接返回 0，不做任何查询。</param>
        /// <param name="results">结果列表（方法内部会先清空）。</param>
        /// <param name="seen">去重集合（方法内部会先清空）。</param>
        /// <returns>找到的敌人数量。</returns>
        public static int FindInRadius(Vector3 center, float radius,
                                       List<IHealth> results, HashSet<IHealth> seen)
        {
            results.Clear();
            seen.Clear();

            if (radius <= 0f)
                return 0;

            int count = Physics.OverlapSphereNonAlloc(
                center, radius, Buffer, EnemyMask, QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                Collider collider = Buffer[i];
                if (collider == null)
                    continue;

                // 用 GetComponentInParent：碰撞体可能挂在子物体/布娃娃骨头上，而血量组件在根节点上。
                IHealth health = collider.GetComponentInParent<IHealth>();
                if (health == null || !health.IsAlive)
                    continue;

                // 去重：同一个敌人即使被多个碰撞体命中，也只处理一次
                if (!seen.Add(health))
                    continue;

                results.Add(health);
            }

            return results.Count;
        }
    }
}