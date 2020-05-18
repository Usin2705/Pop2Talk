using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Coordinate))]
public class CoordinateDrawer : PropertyDrawer {

    const int includeWidth = 50;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return base.GetPropertyHeight(property, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        position = EditorGUI.PrefixLabel(position, label);

        EditorGUIUtility.labelWidth = includeWidth - 10;
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        Rect pos = position;
        pos.xMax -= includeWidth + 50;
        EditorGUI.PropertyField(pos, property.FindPropertyRelative("x"), new GUIContent("X"));

        pos = position;
        pos.xMin = pos.xMax - includeWidth - 50;
        EditorGUI.PropertyField(pos, property.FindPropertyRelative("y"), new GUIContent("Y"));

        EditorGUI.indentLevel = indent;
    }
}