using UnityEngine;

namespace KrolStudio
{
    public static class Easing
    {
        public static float EaseOutExpo(float x)
        {
            return x >= 1f ? 1f : 1f - Mathf.Pow(2f, -10f * x);
        } 
        
        public static float EaseInExpo(float x)
        {
            return x == 0f ? 0f : Mathf.Pow(2f, 10f * x - 10f);
        }

        public static float EaseInQuart(float x)
        {
            return x * x * x * x;
        }

        public static float EaseOutQuart(float x)
        {
            return 1f - Mathf.Pow(1f - x, 4f);
        }

        public static float EaseInQuint(float x)
        {
            return x * x * x * x * x;
        }

        public static float EaseInCubic(float x)
        {
            return x * x * x;
        }

    }
}