using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SpeechCollection.Speech))]
public class SpeechDrawer : PropertyDrawer {

    const int includeWidth = 60;
    const int gap = 5;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return base.GetPropertyHeight(property, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        position = EditorGUI.PrefixLabel(position, label);

        EditorGUIUtility.labelWidth = includeWidth - 22;
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        Rect pos = position;
        pos.xMax -= (includeWidth + gap);
        EditorGUI.PropertyField(pos, property.FindPropertyRelative("speech"), GUIContent.none);
		
        pos = position;
        pos.xMin = pos.xMax - includeWidth - gap;
        EditorGUI.PropertyField(pos, property.FindPropertyRelative("pause"), new GUIContent("Pause"));

        EditorGUI.indentLevel = indent;
    }
}