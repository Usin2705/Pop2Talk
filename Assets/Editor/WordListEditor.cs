using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WordList))]
public class WordLisEditor : Editor {

    public override void OnInspectorGUI() {
        SerializedProperty sizeProperty = serializedObject.FindProperty("size");
        SerializedProperty wordProperty = serializedObject.FindProperty("words");
        SerializedProperty nameProperty = serializedObject.FindProperty("names");
        EditorGUI.BeginChangeCheck();
		EditorGUILayout.DelayedIntField(sizeProperty, new GUIContent(sizeProperty.displayName));
        if (wordProperty.isArray) {
            wordProperty.arraySize = sizeProperty.intValue;
            nameProperty.arraySize = sizeProperty.intValue;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Word");
            EditorGUILayout.LabelField("Name", GUILayout.MinWidth(20.0f), GUILayout.MaxWidth(50.0f), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            int toDeleteIndex = -1;
            for (int i = 0; i < sizeProperty.intValue; ++i) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(wordProperty.GetArrayElementAtIndex(i), GUIContent.none);
                EditorGUILayout.PropertyField(nameProperty.GetArrayElementAtIndex(i), GUIContent.none, GUILayout.MinWidth(20.0f), GUILayout.MaxWidth(50.0f), GUILayout.ExpandWidth(true));
                if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.MaxWidth(16.0f))) {
                    toDeleteIndex = i;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (toDeleteIndex >= 0) {
                wordProperty.GetArrayElementAtIndex(toDeleteIndex).objectReferenceValue = null;
                wordProperty.DeleteArrayElementAtIndex(toDeleteIndex);
                nameProperty.DeleteArrayElementAtIndex(toDeleteIndex);
                --sizeProperty.intValue;
            }
        }
        if (EditorGUI.EndChangeCheck()) {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
