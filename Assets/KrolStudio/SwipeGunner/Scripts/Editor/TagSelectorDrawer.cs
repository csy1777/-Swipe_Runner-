using UnityEngine;
using UnityEditor;

namespace KrolStudio
{
    [CustomPropertyDrawer(typeof(TagSelectorAttribute))]
    public class TagSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
}