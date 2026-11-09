using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace KrolStudio
{
    public abstract class HealthBar : MonoBehaviour
    {
        [SerializeField] protected GameObject holder;
        [SerializeField] private Image fillImage;
        [SerializeField] private Image delayedFillImage;
        [SerializeField] private float reduceSpeed = 0.3f;
        [SerializeField] Transform damageTextPos;

        private CtsHelper ctsHelper = new();

        public Transform DamageTextPos => damageTextPos;
        protected abstract IHealth Health { get; }

        public abstract void OnHealthChanged(object sender, EventArgs e);
        public abstract void OnDied(object sender, EventArgs e);

        private void OnEnable()
        {
            Health.OnHealthChanged += OnHealthChanged;
            Health.OnDied += OnDied;
        }

        public void UpdateHealthInstant()
        {
            fillImage.fillAmount = Health.Ratio;
            delayedFillImage.fillAmount = Health.Ratio;
        }

        protected void UpdateHealthSmooth()
        {
            fillImage.fillAmount = Health.Ratio;

            ctsHelper.Cancel();
            AnimateDelayedBar().Forget();
        }

        private async UniTask AnimateDelayedBar()
        {
            try
            {
                float start = delayedFillImage.fillAmount;
                float end = Health.Ratio;
                float time = 0f;

                while (time < reduceSpeed)
                {
                    float t = time / reduceSpeed;
                    float fillAmount = Mathf.Lerp(start, end, Easing.EaseInQuint(t));
                    delayedFillImage.fillAmount = fillAmount;

                    time += Time.deltaTime;
                    await UniTask.NextFrame(ctsHelper.Token);
                }

                delayedFillImage.fillAmount = Health.Ratio;
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

        private void OnDisable()
        {
            Health.OnHealthChanged -= OnHealthChanged;
            Health.OnDied -= OnDied;
        }

        private void OnDestroy()
        {
            ctsHelper.Dispose();
        }
    }
}