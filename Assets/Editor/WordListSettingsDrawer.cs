using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(WordListSettings))]
public class WordListSettingsDrawer : PropertyDrawer {

    const int includeWidth = 50;
    const int gap = 5;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return base.GetPropertyHeight(property, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        position = EditorGUI.PrefixLabel(position, label);

        EditorGUIUtility.labelWidth = includeWidth - 14;
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        Rect pos = position;
        pos.xMax -= (includeWidth + gap)*2;
        EditorGUI.PropertyField(pos, property.FindPropertyRelative("wordList"), GUIContent.none);

        pos = position;
        pos.xMax -= includeWidth + gap;
        pos.xMin = pos.xMax - includeWidth - gap;
        EditorGUI.PropertyField(pos, property.FindPropertyRelative("first"), new GUIContent("First"));
        
        pos = position;
        pos.xMin = pos.xMax - includeWidth - gap;
        EditorGUI.PropertyField(pos, property.FindPropertyRelative("quizzable"), new GUIContent("Quizzable"));

        EditorGUI.indentLevel = indent;
    }
}