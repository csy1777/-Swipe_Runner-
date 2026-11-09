using System.Collections.Generic;
using UnityEngine;

namespace KrolStudio
{
    [CreateAssetMenu(fileName = "TutorialConfig", menuName = "Configs/TutorialConfig")]
    public class TutorialConfig : ScriptableObject
    {
        [SerializeField] List<PartType> partTypes;

        public PartType GetPartType(int index)
        {
            if(index < 0 || index >= partTypes.Count)
            {
                Debug.LogWarning("Index is out of range. Returning default value.");
                return PartType.None;
            }

            return partTypes[index];
        }
    }
}