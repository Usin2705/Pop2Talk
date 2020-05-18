using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageManager : MonoBehaviour {

	[SerializeField] Language nativeLanguage;
	[SerializeField] Language targetLanguage;
	
	static LanguageManager lm;

	public Language NativeLanguage {
		get {
			return nativeLanguage;
		}

		set {
			nativeLanguage = value;
		}
	}

	public Language TargetLanguage {
		get {
			return targetLanguage;
		}

		set {
			targetLanguage = value;
		}
	}

	public static LanguageManager GetManager() {
		return lm;
	}

	void Awake() {
		lm = this;
	}

	public string GetLanguagePrefix() {
		switch (TargetLanguage) {
			case Language.EnglishUK: return "en_uk_";
			case Language.Finnish: return "fi_";
			default: return "_";
		}
	}
}
