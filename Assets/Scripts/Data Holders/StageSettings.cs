using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stage Settings")]
public class StageSettings : ScriptableObject {

	public LevelSettings level;
	public int starsRequired;
	public int totalCards;
	public int quizCards;
	[Space]
	public WordData[] forcedWords;
	public PopSoundType popSoundType;
	public bool popSoundIsWordForQuiz;
	public int adaptiveWords;
	public int cardPerRound = 1;
	public Sprite background;
	[Space]
	public int day;
	[Space]
	[UniqueIdentifier]
	public string uniqueId;
	[SerializeField]
	bool resetId;
	[SerializeField]
	bool areYouSure;

	void OnValidate() {
		if (resetId) {
			if (areYouSure) {
				resetId = false;
				uniqueId = "";
			}
		} else if (areYouSure)
			areYouSure = false;
	}

	public static bool GetDoneStatus(StageSettings fs, string playerName) {
		return (1 == PlayerPrefs.GetInt(fs.uniqueId + " " + playerName + " done", 0));
	}

	public static void SetDoneStatus(StageSettings fs, string playerName) {
		PlayerPrefs.SetInt(fs.uniqueId + " " + playerName + " done", 1);
	}

	public static int GetStars(StageSettings fs, string playerName) {
		return PlayerPrefs.GetInt(fs.uniqueId + " " + playerName + " stars", 0);
	}
	public static void SetStars(StageSettings fs, int stars, string playerName) {
		PlayerPrefs.SetInt(fs.uniqueId + " " + playerName + " stars", stars);
	}
}