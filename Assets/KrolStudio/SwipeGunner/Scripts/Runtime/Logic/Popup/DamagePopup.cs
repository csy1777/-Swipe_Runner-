using System;
using TMPro;
using UnityEngine;
using DG.Tweening;

namespace KrolStudio
{
    public class DamagePopup : MonoBehaviour, IPoolReturnable<DamagePopup>
    {
        public event Action<DamagePopup> OnReturned;

        [SerializeField] Transform thisTransform;
        [SerializeField] TMP_Text popupText;
        [SerializeField] RectBillboard rectBillboard;
        [SerializeField] float unitSphereRadius = 0.1f;

        [Space]
        [SerializeField] float startDelay;
        [SerializeField] float returnDelay;

        [Space]
        [SerializeField] float rotateTime;
        [Space]
        [SerializeField] float scale;
        [SerializeField] float scaleTime;
        [SerializeField] Ease scaleEase;

        private Vector3 defaultScale;

        void Awake()
        {
            defaultScale = thisTransform.localScale;
        }

        public void Play(int withText, Color withColor)
        {
            popupText.text = $"{withText}";
            popupText.color = withColor;
            StartAnimation();
        }

        private Sequence sequence = null;
        void StartAnimation()
        {
            // Complete and remove the previous animation
            if (sequence != null && sequence.IsActive())
                sequence.Kill();

            Vector3 direction = UnityEngine.Random.onUnitSphere;                    // Get a random direction on the sphere
            direction.z = 0f;                                                       // Zero out Z to keep only X and Y
            direction = direction.normalized;                                       // Re-normalize because zeroing Z changes the length
            Vector3 localOffset = direction * unitSphereRadius;                     // Get offset with specified length
            Vector3 worldOffset = thisTransform.TransformDirection(localOffset);    // Convert to world direction (considering object rotation)
            thisTransform.localPosition += worldOffset;                             // Apply offset to local position

            thisTransform.localRotation = Quaternion.identity;
            thisTransform.localScale = defaultScale;

            Vector3 targetRotation = thisTransform.localRotation.eulerAngles;
            int sign = UnityEngine.Random.value < 0.5f ? 1 : -1;
            targetRotation.z = sign * 19f;

            sequence = DOTween.Sequence();
            sequence
                // First part: rotation + scale
                .Append(thisTransform.DOLocalRotate(targetRotation, rotateTime, RotateMode.LocalAxisAdd).SetEase(scaleEase))
                .Join(thisTransform.DOScale(scale, scaleTime).SetEase(Ease.Linear))

                // Second part: returning to the original position and scale
                .AppendInterval(startDelay)
                .Append(thisTransform.DOLocalRotate(Vector3.zero, rotateTime, RotateMode.LocalAxisAdd).SetEase(scaleEase))
                .Join(thisTransform.DOScale(defaultScale, scaleTime).SetEase(Ease.Linear))

                .AppendInterval(returnDelay)
                .OnComplete(() => ReturnToPool());
        }

        public void ReturnToPool()
        {
            OnReturned?.Invoke(this);
        }
    }
}