using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Infrastructure
{
    [Serializable]
    public struct OverridableField<T>
    {
        public T Value;
        public bool Override;

        public OverridableField(T value, bool doOverride = false)
        {
            Value = value;
            Override = doOverride;
        }

        public readonly OverridableField<T> OverrideWith(OverridableField<T>? overrider = null) =>
            (overrider?.Override ?? false) ? overrider.Value : this;

        public static implicit operator T(OverridableField<T> field) => field.Value;
    }


#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(OverridableField<>))]
    public class OverridableFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var valueProp = property.FindPropertyRelative("Value");
            var overrideProp = property.FindPropertyRelative("Override");

            var toggleRect = new Rect(position.x, position.y, 24, position.height);
            var valueRect = new Rect(position.x + 24, position.y, position.width - 24, position.height);

            int originalIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            overrideProp.boolValue = EditorGUI.Toggle(toggleRect, overrideProp.boolValue);

            EditorGUI.indentLevel = originalIndent;

            bool guiEnabled = GUI.enabled;
            GUI.enabled = overrideProp.boolValue;

            EditorGUI.PropertyField(valueRect, valueProp, label, true);

            GUI.enabled = guiEnabled;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var valueProp = property.FindPropertyRelative("Value");
            return EditorGUI.GetPropertyHeight(valueProp, label);
        }
    }
#endif
}
