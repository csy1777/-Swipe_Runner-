using KrolStudio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace KrolStudio
{
    public class OverHeatUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI overHeatText;
        [SerializeField] private Image overHeatImage;

        TurretShooting turretShooting;

        [Inject]
        void Construct(TurretShooting turretShooting)
        {
            this.turretShooting = turretShooting;
        }

        void Update()
        {
            if (overHeatImage == null || turretShooting == null)
                return;

            overHeatImage.fillAmount = turretShooting.HeatNormalized;

            if (overHeatText != null)
            {
                overHeatText.text = turretShooting.IsOverheated
                    ? "OverHeat"
                    : $"{Mathf.RoundToInt(turretShooting.HeatNormalized * 100f)}%";
            }
        }

        public void DisplayOverHeat(bool value)
        {
            gameObject.SetActive(value);
        }
    }
}
