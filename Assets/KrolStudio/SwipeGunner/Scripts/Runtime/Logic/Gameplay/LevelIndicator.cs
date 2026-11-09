using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KrolStudio
{
    public class LevelIndicator : MonoBehaviour
    {
        [SerializeField] Image levelImage;
        [SerializeField] TextMeshProUGUI levelText;
        [SerializeField] Sprite blueSprite;
        [SerializeField] Sprite purpleSprite;

        public void UpdateLevelIndicator(int level, bool canUpgrade)
        {
            if (canUpgrade)
            {
                levelText.text = (level + 1).ToString();
                levelImage.sprite = blueSprite;
            }
            else
            {
                levelText.text = "MAX";
                levelImage.sprite = purpleSprite;
            }
        }
    }
}