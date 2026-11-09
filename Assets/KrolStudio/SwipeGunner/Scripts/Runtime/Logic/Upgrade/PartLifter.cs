using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace KrolStudio
{
    public class PartLifter : MonoBehaviour
    {
        [SerializeField] float duration = 0.1f;

        private CtsHelper ctsHelper = new();

        public void StartLifting()
        {
            transform.SetParent(null);

            CancelLifting();   // If it's already running — restart it

            LiftPartCoroutine().Forget();
        }

        public void CancelLifting()
        {
            ctsHelper.Restart();
        }

        private async UniTask LiftPartCoroutine()
        {
            try
            {
                // Change only Y to avoid conflicts with DragTarget
                float startY = transform.position.y;
                float endY = startY + 1f;
                float time = 0f;

                while (time < duration)
                {
                    float t = time / duration;
                    float easedY = Mathf.Lerp(startY, endY, Easing.EaseOutQuart(t));
                    transform.position = new Vector3(transform.position.x, easedY, transform.position.z);

                    time += Time.deltaTime;
                    await UniTask.NextFrame(ctsHelper.Token);
                }

                transform.position = new Vector3(transform.position.x, endY, transform.position.z);
            }
            catch (OperationCanceledException)
            {
            }
            catch (MissingReferenceException ex)
            {
                Debug.LogWarning($"The object was destroyed during animation: {ex.Message}");
            }
            catch (NullReferenceException ex)
            {
                Debug.LogWarning($"Missing link during animation: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error: {ex}");
            }
        }

        private async UniTask LiftPartCoroutineV2(Vector3 hitPoint)
        {
            try
            {
                float time = 0f;
                float prevOffset = 0f;

                Vector3 direction = hitPoint - transform.position;
                direction.y = 1f; // Move up by 1 on the Y axis

                while (time < duration)
                {
                    time += Time.deltaTime;
                    float t = Mathf.Clamp01(time / duration);

                    float currentOffset = Easing.EaseOutQuart(t);
                    float deltaOffset = currentOffset - prevOffset;

                    transform.position += direction * deltaOffset;

                    prevOffset = currentOffset;
                    await UniTask.NextFrame(ctsHelper.Token);
                }
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