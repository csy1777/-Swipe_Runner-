using UnityEngine;

namespace KrolStudio
{
    [RequireComponent(typeof(PartLevelController), typeof(PartVisualsController))]
    public class UpgradeLevelSwitcher : MonoBehaviour
    {
        [SerializeField] PartType partType;

        private PartLevelController levelController;
        private PartVisualsController visualsController;

        public PartType PartType => partType;

        private void Awake()
        {
            levelController = GetComponent<PartLevelController>();
            visualsController = GetComponent<PartVisualsController>();
        }

        public int PlayerPartLevel => levelController.Level;

        public void Initialize(int level) 
        {
            levelController.Initialize(partType, level);

            visualsController.Initialize(levelController.FindCurrentPart(), level);

            visualsController.UpdateLevelIndicator(levelController.Level, levelController.CanUpgrade());
            visualsController.DisplayLevelIndicator(!(levelController.Level < 0));
        }

        public bool CanUpgradePlayerPart()
        {
            return levelController.CanUpgrade();
        }

        public void PlayUpgradeEffect()
        {
            visualsController.PlayEffect();
        }

        public void EnablePreviewBacklight()
        {
            visualsController.EnablePreviewBacklight();
        }

        public void DisablePreviewBacklight()
        {
            visualsController.DisablePreviewBacklight();
        }

        public void EnableBacklight(int partLevel) 
        {
            visualsController.EnableBacklight(partLevel >= levelController.Level && levelController.CanUpgrade());
        }

        public void DisableBacklight()
        {
            visualsController.DisableBacklight();
        }

        public void DisplayLevelIndicator(bool value)
        {          
            visualsController.DisplayLevelIndicator(value);
        }
    }
}