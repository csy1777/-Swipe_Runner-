using UnityEngine;

namespace KrolStudio
{
    [CreateAssetMenu(fileName = "FragmentExplosion", menuName = "Configs/FragmentExplosion")]
    public class FragmentExplosionConfig : ScriptableObject
    {
        public float angleInDegrees = 60f;
        public int rotationCount = 10;

        [Space]
        public float minSpawnAngle = -90f;
        public float maxSpawnAngle = 90f;
        public float firstRadius = 4f;
        public float secondRadius = 8f;

        [Space]
        public int trajectoryPoints = 30;
        public float timeStep = 0.1f;

        [Space]
        public Material grayMaterial;
    }
}