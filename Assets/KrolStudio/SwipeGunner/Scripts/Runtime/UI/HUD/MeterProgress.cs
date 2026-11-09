using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KrolStudio
{
    public class MeterProgress : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI meterText;
        [SerializeField] Slider meterSlider;

        public void SetProgressView(float value)
        {
            meterSlider.value = value;
        }

        public void SetTraveledDistance(float meter)
        {
            meterText.text = $"{meter}m";
        }
    }
}