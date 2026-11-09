using TMPro;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class AmmoBoxPart : MonoBehaviour
    {
        [SerializeField] PartLevelController partLevel;
        [SerializeField] TMP_Text bulletAmount;

        IPlayerContext playerContext;

        [Inject]
        void Construct(IPlayerContext playerContext)
        {
            this.playerContext = playerContext;
        }

        private void LevelChanged()
        {
            bulletAmount.text = playerContext.PlayerStats.CalculateAmmoAmount(partLevel.Level).ToString();
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