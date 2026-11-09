using UnityEngine;

namespace KrolStudio
{
    public class TutorialHand : MonoBehaviour
    {
        [SerializeField] Animator handClick;
        [SerializeField] Animator handUpgrade;
        [SerializeField] Animator handPlayStart;

        public void PlayStartClick()
        {
            handUpgrade.gameObject.SetActive(false);
            handClick.gameObject.SetActive(false);

            handPlayStart.gameObject.SetActive(true);
            handPlayStart.Play("HandClick", -1, 0f);
        }

        public void PlayClick()
        {
            handUpgrade.gameObject.SetActive(false);

            handClick.gameObject.SetActive(true);
            handClick.Play("HandClick", -1, 0f);
        }

        public void PlayTap()
        {
            handClick.gameObject.SetActive(false);

            handUpgrade.gameObject.SetActive(true);
            handUpgrade.Play("HandTap", -1, 0f);
        }

        public void StopTutorial()
        {
            handUpgrade.gameObject.SetActive(false);
            handClick.gameObject.SetActive(false);
            handPlayStart.gameObject.SetActive(false);
        }
    }
}