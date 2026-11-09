using TMPro;
using UnityEngine;

namespace KrolStudio
{
    public class TextAnimator : MonoBehaviour
    {
        public TextMeshProUGUI tmp;

        public void SetText(string value)
        {
            tmp.text = value;
        }
    }
}