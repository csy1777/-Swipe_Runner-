using UnityEngine;

namespace KrolStudio
{
    public static class RandomService
    {
        public static Vector3 InsideUnitSphereGaussian(float mean = 0f, float stdDev = 1f)
        {
            Vector3 direction = Random.onUnitSphere;
            float radius = NextGaussian(mean, stdDev);
            return direction * radius;
        }

        public static float NextGaussian(float mean = 0f, float stdDev = 1f)
        {
            float u1 = 1.0f - UnityEngine.Random.value;
            float u2 = 1.0f - UnityEngine.Random.value;
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
            return mean + stdDev * randStdNormal;
        }
    }
}