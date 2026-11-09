using UnityEngine;
using TMPro;
using System;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;

namespace KrolStudio
{
    public class PopUpText : MonoBehaviour
    {
        [SerializeField] TMP_Text popupText;
        [SerializeField] CanvasGroup cg;

        [Space]
        [SerializeField] float moveDuration;
        [SerializeField] float moveDistance;
        [SerializeField] float startDelay;

        [Space]
        [SerializeField][Range(0.0001f, 1f)] float fadeOutStartTimeOnCurve = 0.15f;
        [SerializeField][Range(0f, 1f)] float fadeInStartTimeOnCurve = 0.7f;
        [SerializeField] AnimationCurve moveCurve;

        [Space]
        [SerializeField] float timeToDestruction;

        [Space]

        [SerializeField] UnityEvent OnStartAnimation;
        [SerializeField] UnityEvent OnEndAnimation;

        public event Action OnFinalize;

        private CtsHelper ctsHelper = new();

        public void Play(string withText)
        {
            popupText.text = withText;
            StartAnimation().Forget();
        }

        public void Play()
        {
            StartAnimation().Forget();
        }

        private async UniTask StartAnimation()
        {
            OnStartAnimation.Invoke();

            cg.alpha = 0f;
            await UniTask.Delay(TimeSpan.FromSeconds(startDelay));
            cg.alpha = 1f;

            TextAnimation().Forget();
        }

        virtual public async UniTask TextAnimation()
        {
            try
            {
                Vector3 startPos = transform.localPosition;
                Vector3 endPos = startPos + Vector3.up * moveDistance;

                float elapsed = 0f;
                cg.alpha = 1;

                while (elapsed < moveDuration)
                {
                    float t = elapsed / moveDuration;

                    transform.localPosition = Vector3.Lerp(startPos, endPos, moveCurve.Evaluate(t));
                    cg.alpha = Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, fadeOutStartTimeOnCurve, t)); // normalize from fadeStartTimeOnCurve to 1

                    // Fading
                    if (t >= fadeInStartTimeOnCurve)
                    {
                        cg.alpha = Mathf.Lerp(1f, 0f, Mathf.InverseLerp(fadeInStartTimeOnCurve, 1f, t)); // normalize from fadeStartTimeOnCurve to 1
                    }

                    elapsed += Time.deltaTime;
                    await UniTask.NextFrame(ctsHelper.Token);
                }

                transform.localPosition = endPos;
                cg.alpha = 0;

                await UniTask.Delay(TimeSpan.FromSeconds(timeToDestruction));

                OnFinalize?.Invoke();
                OnEndAnimation?.Invoke();
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