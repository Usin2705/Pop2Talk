using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Word List")]
public class WordList : ScriptableObject {
    [SerializeField] int size;
    [SerializeField] WordData[] words;
    [SerializeField] string[] names;

    void OnValidate() {
        for(int i = 0; i < words.Length;++i) {
            if (words[i] != null) {
                names[i] = words[i].name;
                words[i] = null;
            }
        }
    }

    public string[] GetWords() {
        return names;
    }

}
