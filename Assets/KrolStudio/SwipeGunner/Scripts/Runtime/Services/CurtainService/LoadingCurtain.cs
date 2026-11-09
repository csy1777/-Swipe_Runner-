using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace KrolStudio
{
    public class LoadingCurtain : MonoBehaviour
    {
        [SerializeField] float fadeInDuration = 1f;
        [SerializeField] CanvasGroup cg;
        [SerializeField] Image dotsBackground;

        private float alphaControl;
        private Material dotsMaterial;
        private CtsHelper ctsHelper = new();

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            dotsMaterial = new Material(dotsBackground.material);
            dotsBackground.material = dotsMaterial;
            alphaControl = dotsMaterial.GetFloat("_AlphaControl");
        }

        public void Show()
        {
            gameObject.SetActive(true);
            cg.alpha = 1f;
            dotsMaterial.SetFloat("_AlphaControl", alphaControl);
        }

        public void Hide() => DoFadeIn().Forget();


        private async UniTask DoFadeIn()
        {
            try
            {
                float elapsed = 0f;
                float startAlpha = cg.alpha;

                while (elapsed < fadeInDuration)
                {
                    elapsed += Time.deltaTime;

                    cg.alpha = Mathf.Lerp(startAlpha, 0f, Easing.EaseInCubic(elapsed / fadeInDuration));
                    dotsMaterial.SetFloat("_AlphaControl", Mathf.Lerp(0f, 0.35f, Easing.EaseInCubic(cg.alpha)));
                    await UniTask.NextFrame(ctsHelper.Token);
                }

                cg.alpha = 0f;
                dotsMaterial.SetFloat("_AlphaControl", 0f);
                gameObject.SetActive(false);
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

            if (dotsMaterial != null)
            {
                Destroy(dotsMaterial);
                dotsMaterial = null;
            }
        }
    }
}