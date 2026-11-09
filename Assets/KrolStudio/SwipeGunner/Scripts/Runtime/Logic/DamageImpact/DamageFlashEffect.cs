using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KrolStudio
{
    public class DamageFlashEffect : MonoBehaviour
    {
        [SerializeField] List<Renderer> targetRenderer;
        [SerializeField] float flashDuration = 0.1f;
        [SerializeField] Material whiteFlashMaterial;

        private List<Material> originalMaterial;
        private CtsHelper ctsHelper = new();

        /// <summary>
        /// 持续状态用的覆盖材质（例如冰冻时把敌人染成冰蓝）。
        ///
        /// 为什么放在这里而不是另写一个组件：
        ///   这个组件本来就持有"渲染器列表 + 原始材质"，是外观的唯一管理者。
        ///   如果冰冻自己换材质，命中闪白结束时 ResetMaterial 会恢复成原始材质，
        ///   把冰蓝色抹掉；反过来冰冻结束时又会盖掉正在进行的闪白。
        ///   让两者共用同一个"当前该用哪个材质"的判断，就不会互相打架。
        ///
        /// 优先级：命中闪白（临时） > 持续材质（冰冻） > 原始材质。
        /// </summary>
        private Material persistentMaterial;

        void Start()
        {
            originalMaterial = new List<Material>(targetRenderer.Count);
            for (int i = 0; i < targetRenderer.Count; i++)
                originalMaterial.Add(targetRenderer[i].material);
        }

        public void CancelFlashWhite()
        {
            ctsHelper.Cancel();
        }

        public void FlashWhite()
        {
            ctsHelper.Restart();
            FlashCoroutine().Forget();
        }

        /// <summary>
        /// 设置持续材质（例如冰冻）。传 null 等于"没有覆盖"。
        /// 会立即刷新外观；正在进行中的命中闪白不会被它打断，但闪白结束后会回到这个材质。
        /// </summary>
        public void SetPersistentMaterial(Material material)
        {
            persistentMaterial = material;
            ResetMaterial();
        }

        /// <summary>清除持续材质，恢复成原始材质。</summary>
        public void ClearPersistentMaterial()
        {
            persistentMaterial = null;
            ResetMaterial();
        }

        void OnDisable()
        {
            ctsHelper.Dispose();
        }

        void SetMaterial(Material mat)
        {
            for (int i = 0; i < targetRenderer.Count; i++)
                targetRenderer[i].material = mat;
        }

        void ResetMaterial()
        {
            // Start 还没跑过（理论上不会发生）时先跳过，避免 originalMaterial 为 null
            if (originalMaterial == null)
                return;

            for (int i = 0; i < targetRenderer.Count; i++)
            {
                // 有持续材质（冰冻）就用它，否则回到原始材质
                targetRenderer[i].material = persistentMaterial != null
                    ? persistentMaterial
                    : originalMaterial[i];
            }
        }

        async UniTask FlashCoroutine()
        {
            try
            {
                SetMaterial(whiteFlashMaterial);
                await UniTask.Delay(TimeSpan.FromSeconds(flashDuration), cancellationToken: ctsHelper.Token);
                ResetMaterial();

                await UniTask.Delay(TimeSpan.FromSeconds(flashDuration), cancellationToken: ctsHelper.Token);

                SetMaterial(whiteFlashMaterial);
                await UniTask.Delay(TimeSpan.FromSeconds(flashDuration), cancellationToken: ctsHelper.Token);
                ResetMaterial();
            }
            catch (OperationCanceledException)
            {
            }
            catch (MissingReferenceException ex)
            {
                Debug.LogWarning($"The object was destroyed during animation: {ex.Message}", this);
            }
            catch (NullReferenceException ex)
            {
                Debug.LogWarning($"Missing link during animation: {ex.Message}", this);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error: {ex}", this);
            }
        }

        private void OnDestroy()
        {
            ctsHelper.Dispose();
        }
    }
}