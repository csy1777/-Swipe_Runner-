using UnityEditor;
using UnityEngine;

namespace KrolStudio
{
    public class RemovePrefix : EditorWindow
    {
        private GameObject targetObject;
        private string prefixToRemove = "";

        [MenuItem("Tools/Remove Prefix from Children")]
        public static void ShowWindow()
        {
            GetWindow<RemovePrefix>("Remove Prefix");
        }

        private void RemovePrefixFromChildren(GameObject targetObject, string prefix)
        {
            foreach (Transform child in targetObject.transform)
            {
                if (child.name.StartsWith(prefix))
                {
                    child.name = child.name.Substring(prefix.Length);
                }

                RemovePrefixFromChildren(child.gameObject, prefix);
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("Remove Prefix from Children", EditorStyles.boldLabel);

            targetObject = (GameObject)EditorGUILayout.ObjectField("Target Root Object", targetObject, typeof(GameObject), true);

            prefixToRemove = EditorGUILayout.TextField("Prefix to Remove", prefixToRemove);

            if (GUILayout.Button("Remove Prefix from Children"))
            {
                if (targetObject != null && !string.IsNullOrEmpty(prefixToRemove))
                {
                    RemovePrefixFromChildren(targetObject, prefixToRemove);
                    Debug.Log($"Removed prefix '{prefixToRemove}' from all children of {targetObject.name}");
                }
                else
                {
                    Debug.LogError("Please specify both the target object and the prefix.");
                }
            }
        }
    }
}