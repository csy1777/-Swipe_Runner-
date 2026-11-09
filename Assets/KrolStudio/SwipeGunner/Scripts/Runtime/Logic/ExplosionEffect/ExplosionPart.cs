using UnityEngine;

namespace KrolStudio
{
    public class ExplosionPart : ExplosionEffectHandler
    {
        [SerializeField] PartLevelController levelController;

        private void OnEnable()
        {
            Initialize(levelController);
        }
    }
}