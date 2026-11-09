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
            for (int i = 0; i < targetRenderer.Count; i++)
                targetRenderer[i].material = originalMaterial[i];
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