using System.Collections.Generic;
using UnityEngine;

namespace KrolStudio
{
    [CreateAssetMenu(fileName = "LevelList", menuName = "Configs/LevelList")]
    public class LevelList : ScriptableObject
    {
        [SerializeField] List<Level> levels;

        //public IReadOnlyList<Level> Levels => levels;

        public int Count => levels.Count;

        public Level GetLevel(int index)
        {
            if (index < 0 || index >= levels.Count)
            {
                Debug.LogError("IndexError: list index out of range.", this);
                return null;
            }

            return levels[index];
        }
    }
}