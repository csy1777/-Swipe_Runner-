using System.Collections.Generic;
using UnityEngine;

namespace KrolStudio
{
    public class ExplosionEffectHandler : MonoBehaviour
    {
        [SerializeField] List<ExplosionPartInfo> explosionInfo = new();

        PartLevelController _levelController;

        public void Initialize(PartLevelController levelController)
        {
            this._levelController = levelController;
        }

        public List<Rigidbody> ExplodeParts()
        {
            if (Index < 0) return null;

            if(Index >= explosionInfo.Count)
            {
                Debug.LogError($"Explosion component missing. Check ExplosionPart/ExplosionEffectHandler on {gameObject.name}.", this);
                return null;
            }

            return explosionInfo[Index].explodeParts;
        }

        public List<Renderer> ExplodeRenderer()
        {
            if (Index < 0) return null;

            if (Index >= explosionInfo.Count)
            {
                Debug.LogError($"Explosion component missing. Check ExplosionPart/ExplosionEffectHandler on {gameObject.name}.", this);
                return null;
            }

            return explosionInfo[Index].explodeRenderer;
        }

        int Index => _levelController == null ? 0 : _levelController.Level;

        [System.Serializable]
        class ExplosionPartInfo
        {
            public List<Rigidbody> explodeParts;
            public List<Renderer> explodeRenderer;
        }
    }
}