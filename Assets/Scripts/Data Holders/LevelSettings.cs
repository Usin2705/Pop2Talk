using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Level Settings")]
public class LevelSettings : ScriptableObject {

	public GameMode gameType;
	public float medalValuePerRound;
	public bool trackClicks;
	public int movesPerCard;
	[Space]
	public LevelTypeSettings levelTypeSettings;
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

	public static int GetStars(LevelSettings ls, string playerName) {
		return PlayerPrefs.GetInt(ls.uniqueId + " " + playerName + " stars", 0);
	}

	public static bool GetDoneStatus(LevelSettings ls, string playerName) {
		return (1 == PlayerPrefs.GetInt(ls.uniqueId + " " + playerName + " done", 0));
	}


	public static void SetStars(LevelSettings ls, int stars, string playerName, bool overwriteStars = false) {
		if (!overwriteStars)
			stars = Mathf.Max(stars, GetStars(ls, playerName));
		stars = Mathf.Max(0, stars);
		PlayerPrefs.SetInt(ls.uniqueId + " " + playerName + " stars", stars);
	}

	public static void SetDoneStatus(LevelSettings ls, bool done, string playerName, bool overwriteDone = false) {
		done = done || (!overwriteDone && GetDoneStatus(ls, playerName));
		PlayerPrefs.SetInt(ls.uniqueId + " " + playerName + " done", (done) ? 1 : 0);
	}

	public static void SetMedal(LevelSettings ls, bool medal, string playerName, bool overwriteDone = false) {
		medal = medal || (!overwriteDone && GetMedal(ls, playerName));
		PlayerPrefs.SetInt(ls.uniqueId + " " + playerName + " medal", (medal) ? 1 : 0);
	}

	public static bool GetMedal(LevelSettings ls, string playerName) {
		return (1 == PlayerPrefs.GetInt(ls.uniqueId + " " + playerName + " medal", 0));
	}

	public static void SetMedal(LevelSettings ls, string playerName) {
		PlayerPrefs.SetInt(ls.uniqueId + " " + playerName + " medal", 1);
	}

	public static void SetHiscore(LevelSettings ls, int score, string playerName) {
		PlayerPrefs.SetInt(ls.uniqueId + " " + playerName + " score", score);
	}

	public static int GetHiscore(LevelSettings ls, string playerName) {
		return PlayerPrefs.GetInt(ls.uniqueId + " " + playerName + " score", 0);
	}
}