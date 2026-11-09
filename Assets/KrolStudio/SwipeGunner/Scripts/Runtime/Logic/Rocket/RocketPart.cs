using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class RocketPart : MonoBehaviour
    {
        [SerializeField] PartLevelController partLevel;
        [SerializeField] List<MeshRenderer> rocketsRenderer;
        [SerializeField] List<Color> rocketColors;

        IPlayerContext playerContext;

        [Inject]
        void Construct(IPlayerContext playerContext)
        {
            this.playerContext = playerContext;
        }

        private void LevelChanged()
        {
            if (partLevel.Level < 0) return;

            foreach (var renderer in rocketsRenderer)
            {
                renderer.material.color = rocketColors[partLevel.Level];
            }
        }

        private void OnEnable()
        {
            partLevel.OnLevelChanged += LevelChanged;
        }

        private void OnDisable()
        {
            partLevel.OnLevelChanged -= LevelChanged;
        }
    }
}