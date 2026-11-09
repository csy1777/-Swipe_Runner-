using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace KrolStudio
{
    public class CurveMover : MonoBehaviour
    {
        [SerializeField] float duration = 0.05f;    // Duration of the movement

        private CtsHelper ctsMoveHelper = new();
        private CtsHelper ctsScaleHelper = new();

        // Starts moving the object along a curve from its current position to the specified one.
        public void MoveTo(Transform targetPosition, Func<float, float> easingFunction = null, Action action = null)
        {
            ctsMoveHelper.Restart();
            MoveByCurve(transform.position, targetPosition.position, easingFunction, action).Forget();
        }

        public void ScaleTo(Vector3 targetScale, Func<float, float> easingFunction = null, Action action = null)
        {
            ctsScaleHelper.Restart();
            ScaleByCurve(transform.localScale, targetScale, easingFunction, action).Forget();
        }

        // Interrupts the current movement.
        public void Cancel()
        {
            ctsMoveHelper.Cancel();
        }

        private async UniTask AnimateByCurve(Vector3 from, Vector3 to, Action<Vector3> setter, CtsHelper ctsHelper, Func<float, float> easingFunction = null, Action callback = null)
        {
            float time = 0f;

            // If easingFunction is not provided — default to linear interpolation
            if (easingFunction == null)
                easingFunction = t => t;

            try
            {
                while (time < duration)
                {
                    float t = time / duration;
                    float curvedT = easingFunction(t);

                    Vector3 value = Vector3.LerpUnclamped(from, to, curvedT);
                    setter(value);

                    time += Time.deltaTime;
                    await UniTask.NextFrame(ctsHelper.Token);
                }

                setter(to);
                callback?.Invoke();
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

        private async UniTask MoveByCurve(Vector3 from, Vector3 to, Func<float, float> easingFunction = null, Action callback = null)
        {
            await AnimateByCurve(from, to, value => transform.position = value, ctsMoveHelper, easingFunction, callback);
        }

        private async UniTask ScaleByCurve(Vector3 from, Vector3 to, Func<float, float> easingFunction = null, Action callback = null)
        {
            await AnimateByCurve(from, to, value => transform.localScale = value, ctsScaleHelper, easingFunction, callback);
        }

        private void OnDestroy()
        {
            ctsMoveHelper.Dispose();
            ctsScaleHelper.Dispose();
        }
    }
}