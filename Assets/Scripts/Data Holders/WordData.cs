using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Language { None, Finnish, EnglishUK};

[System.Serializable]
public struct AudioString {
    public string spelling;
    public AudioClip[] pronunciations;

    public AudioString(AudioClip[] pronunciations, string spelling) {
        this.pronunciations = pronunciations;
        this.spelling = spelling;
    }
}

[System.Serializable]
public class Prompts {
    public LanguageAudioClipDictionary prompts;
}

[CreateAssetMenu(fileName = "Word Data")]
public class WordData : ScriptableObject {

	public Sprite picture;
    public LanguageAudioStringDictionary languageWords;
}
