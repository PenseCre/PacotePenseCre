using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

namespace PacotePenseCre.BuildPipeline
{
    [CustomPropertyDrawer(typeof(BuildSetting), true)]
	public class BuildSettingPropertyDrawer : PropertyDrawer
	{
        private const float lineSeparation = 2f;
        private const float buildSettingTypeWidth = 84f;
        private const float buildSettingTypeSeparation = 4f;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 2f;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{

            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            position.height /= 2f;
            var keyRect = new Rect(position.x, position.y, position.width, position.height - lineSeparation);
            var typeRect = new Rect(position.x, position.y + position.height, buildSettingTypeWidth, position.height - lineSeparation);
            var valueRect = new Rect(position.x + buildSettingTypeWidth + buildSettingTypeSeparation, position.y + position.height, position.width - buildSettingTypeWidth - buildSettingTypeSeparation, position.height - lineSeparation);

            var typeProp = property.FindPropertyRelative("type");
            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(keyRect, property.FindPropertyRelative("key"), GUIContent.none);
            EditorGUI.PropertyField(typeRect, typeProp, GUIContent.none);

            BuildSettingSupported type = (BuildSettingSupported)typeProp.enumValueIndex;
            EditorGUI.PropertyField(valueRect, property.FindPropertyRelative(type.ToString()), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}