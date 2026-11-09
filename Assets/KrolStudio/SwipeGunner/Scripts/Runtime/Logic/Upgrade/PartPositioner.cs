using UnityEngine;

namespace KrolStudio
{
    public class PartPositioner : MonoBehaviour
    {
        [SerializeField] PartLevelController primaryPart;
        [SerializeField] PartLevelController dependentPart;

        [Space]
        [SerializeField] PartPositionSet[] positionsSet;

        private void Awake()
        {
            primaryPart.OnLevelChanged += UpdateDependentPartPosition;
            dependentPart.OnLevelChanged += UpdateDependentPartPosition;
        }

        private void OnDisable()
        {
            primaryPart.OnLevelChanged -= UpdateDependentPartPosition;
            dependentPart.OnLevelChanged -= UpdateDependentPartPosition;
        }

        private void UpdateDependentPartPosition()
        {
            if(dependentPart.Level < 0 || dependentPart.Level >= positionsSet.Length)
            {
                Debug.LogWarning($"Missing Part - Level {dependentPart.Level}. Check the PartPositioner component.");
                return;
            }

            if (primaryPart.Level < 0 || primaryPart.Level >= positionsSet[dependentPart.Level].orientations.Length)
            {
                SetPartTransform(dependentPart.Level, positionsSet[dependentPart.Level].defaultTransform);
            }
            else
            {
                SetPartTransform(dependentPart.Level, positionsSet[dependentPart.Level].orientations[primaryPart.Level]);
            }
        }

        private void SetPartTransform(int dependentIndex, PartTransformData data)
        {
            positionsSet[dependentIndex].dependentTransform.localPosition = data.positions;
            positionsSet[dependentIndex].dependentTransform.localRotation = Quaternion.Euler(data.rotation);
            positionsSet[dependentIndex].dependentTransform.localScale = data.scale;
        }
    }

    [System.Serializable]
    public class PartPositionSet
    {
        public Transform dependentTransform;
        public PartTransformData defaultTransform;
        public PartTransformData[] orientations;
    }

    [System.Serializable]
    public class PartTransformData
    {
        public Vector3 positions = Vector3.zero;
        public Vector3 rotation = Vector3.zero;
        public Vector3 scale = Vector3.one;
    }
}