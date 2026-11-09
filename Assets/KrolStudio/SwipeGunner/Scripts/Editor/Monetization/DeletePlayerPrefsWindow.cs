using UnityEngine;
using UnityEditor;

namespace KrolStudio
{
    public class DeletePlayerPrefsWindow : EditorWindow
    {
        [MenuItem("Tools/Monetization/Delete PlayerPrefs")]
        public static void ShowWindow()
        {
            if (EditorUtility.DisplayDialog("Confirmation", "Are you sure you want to delete PlayerPrefs?", "Yes", "No"))
            {
                PlayerPrefs.DeleteAll();
            }
        }
    }
}