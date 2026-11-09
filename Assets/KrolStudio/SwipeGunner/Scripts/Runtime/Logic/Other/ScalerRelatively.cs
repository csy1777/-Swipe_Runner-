using System.Collections.Generic;
using UnityEngine;

namespace KrolStudio
{
    public class ScalerRelatively : MonoBehaviour
    {
        [SerializeField] List<Transform> targets;

        public bool stop = true;

        Canvas rootCanvas;

        public Vector2 referenceResolution = new Vector2(1080, 1920);
        [Range(0f, 1f)] public float matchWidthOrHeight = 1f;
        public float kLogBase = 2.0f; // Logarithm base (default is 2)

        void Update()
        {
            if (stop) return;

            int screenWidth = Screen.width;
            int screenHeight = Screen.height;

            float scale = GetScale(screenWidth, screenHeight, referenceResolution, matchWidthOrHeight);
            float scale2 = GetScale2(screenWidth, screenHeight, referenceResolution, matchWidthOrHeight);

            Debug.Log(scale + " " + scale2);

            foreach (Transform t in targets)
            {
                t.localScale =new Vector3(scale, scale, 1);
            }
        }

        private float GetScale(int width, int height, Vector2 scalerReferenceResolution, float matchWidthOrHeight)
        {
            return Mathf.Pow(width / scalerReferenceResolution.x, 1f - matchWidthOrHeight) *
                   Mathf.Pow(height / scalerReferenceResolution.y, matchWidthOrHeight);
        }

        float GetScale2(int width, int height, Vector2 scalerReferenceResolution, float matchWidthOrHeight)
        {
            float logWidth = Mathf.Log(width / referenceResolution.x, kLogBase);
            float logHeight = Mathf.Log(height / referenceResolution.y, kLogBase);

            float logWeightedAverage = Mathf.Lerp(logWidth, logHeight, matchWidthOrHeight);

            float scaleFactor = Mathf.Pow(kLogBase, logWeightedAverage);

            return scaleFactor;
        }
    }
}