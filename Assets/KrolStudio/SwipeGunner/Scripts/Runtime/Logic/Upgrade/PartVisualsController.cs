using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KrolStudio
{
    public class PartVisualsController : MonoBehaviour
    {
        [Header("Particles")]
        [SerializeField] private ParticleSystem mergeFX;       

        [Header("Backlight")]
        [SerializeField] private Material greenMat;
        [SerializeField] private Material redMat;

        [Space]
        [SerializeField] private LevelIndicator levelIndicator;

        const string IgnoreMaterialBackup = "IgnoreMaterialBackup";

        private List<Material> originalMaterials;
        private Renderer[] currentRenderers;
        private GameObject currentPart;
        private CtsHelper ctsHelper = new();

        private void OnEnable()
        {
            // It is necessary for the correct initialization of the LineRenderer from LevelIndicator on the player's context.
            // Start of initialization UpgradeLevelController.ApplyPartLevelDelayed
            DisplayLevelIndicator(false);
        }

        public void Initialize(GameObject part, int level)
        {
            if (level >= 0)
            {
                DisablePart();

                part.SetActive(true);
            }
            
            currentPart = part;
            CollectOriginalMaterials(part);
        }

        public void DisplayLevelIndicator(bool value)
        {
            if (this == null || transform == null || levelIndicator == null)
                return;
            levelIndicator.gameObject.SetActive(value);
        }

        public void UpdateLevelIndicator(int level, bool canUpgrade)
        {
            levelIndicator.UpdateLevelIndicator(level, canUpgrade);
        }

        private void DisablePart()
        {
            currentPart?.SetActive(false);
        }

        private void CollectOriginalMaterials(GameObject currentPart)
        {
            currentRenderers = currentPart.GetComponentsInChildren<MeshRenderer>(true);
            originalMaterials = new List<Material>();

            foreach (var mr in currentRenderers)
            {
                if (mr.gameObject.CompareTag(IgnoreMaterialBackup))
                    continue;

                foreach (var mat in mr.materials)
                    originalMaterials.Add(mat);
            }
        }

        public void EnablePreviewBacklight()
        {
            currentPart?.SetActive(true);
            EnableBacklight(true);
        }

        public void DisablePreviewBacklight()
        {
            DisableBacklight();
            currentPart?.SetActive(false);
        }

        public void EnableBacklight(bool isGreen)
        {
            SetBacklightMaterial(isGreen ? greenMat : redMat);
        }

        public void DisableBacklight()
        {
            RestoreOriginalMaterials();
        }

        private void SetBacklightMaterial(Material mat)
        {
            Material[] mats;
            foreach (var mr in currentRenderers)
            {
                if (mr.gameObject.CompareTag(IgnoreMaterialBackup))
                    continue;

                mats = new Material[mr.materials.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = mat;
                }
                mr.materials = mats;
            }
        }

        private void RestoreOriginalMaterials()
        {
            int index = 0;
            Material[] mats;
            foreach (var mr in currentRenderers)
            {
                if (mr.gameObject.CompareTag(IgnoreMaterialBackup))
                    continue;

                mats = new Material[mr.materials.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = originalMaterials[index++];
                }
                mr.materials = mats;
            }
        }

        public void PlayEffect()
        {
            mergeFX.Play();
            MergeScaleAnim().Forget();
        }

        private async UniTask MergeScaleAnim()
        {
            float startScale = transform.localScale.x;
            float targetScale = startScale + 0.15f;

            await ScaleOverTime(startScale, targetScale, 0.1f);
            await ScaleOverTime(targetScale, startScale, 0.05f);

            transform.localScale = Vector3.one * startScale;
        }

        private async UniTask ScaleOverTime(float from, float to, float duration)
        {
            try
            {
                float time = 0f;

                while (time < duration)
                {
                    float t = time / duration;
                    float scale = Mathf.Lerp(from, to, t);
                    transform.localScale = Vector3.one * scale;
                    time += Time.deltaTime;

                    await UniTask.NextFrame(ctsHelper.Token);
                }
                
                transform.localScale = Vector3.one * to;
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