using UnityEditor;
using UnityEngine;

namespace KrolStudio
{
    public class Tools
    {
        [MenuItem("Tools/Clear Save Data")]
        public static void ClearData()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}