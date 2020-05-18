using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetManager : MonoBehaviour {

	[SerializeField] PlanetSettings[] planets;

	static PlanetManager pm;

	int currentPlanetIndex;
	int currentStageIndex;

	public static PlanetManager GetManager() {
		return pm;
	}

	void Awake() {
		pm = this;
	}

	public void SetPlanet(int index) {
		if (index < planets.Length) {
			currentPlanetIndex = index;
		} else {
			Debug.LogError("Trying to set planet out of index");
		}

	}

	public void SetStage(int index) {
		if (index < GetStageCount()) {
			currentStageIndex = index;
		} else {
			Debug.LogError("Trying to set planet out of index");
		}

	}

	public int GetPlanetIndex() {
		return currentPlanetIndex;
	}

	public Sprite[] GetPlanetSprites() {
		Sprite[] sprites = new Sprite[planets.Length];
		for (int i = 0; i < planets.Length; ++i) {
			sprites[i] = planets[i].planetSprite;
		}
		return sprites;
	}

	public int GetStageCount() {
		return planets[currentPlanetIndex].stages.Length;
	}

	public GameObject GetNodeLayoutPrefab() {
		return planets[currentPlanetIndex].nodeLayoutPrefab;
	}

	public string[] GetPlanetStats() {
		string[] planetStats = new string[2];
		int stars = 0, starsMax = 0, medals = 0;
		for (int i = 0; i < planets[currentPlanetIndex].stages.Length; ++i) {
			stars += StageSettings.GetStars(planets[currentPlanetIndex].stages[i], NetworkManager.GetManager().Player);
			starsMax += planets[currentPlanetIndex].stages[i].totalCards * ConstantHolder.maxStars;
			if (LevelSettings.GetMedal(planets[currentPlanetIndex].stages[i].level, NetworkManager.GetManager().Player))
				medals++;
		}
		planetStats[0] = stars + " / " + starsMax;
		planetStats[1] = medals + " / " + planets[currentPlanetIndex].stages.Length;
		return planetStats;
	}

	public string[] GetStageStats(bool useHiscore = true, int stars = 0, int trackedValue = 0) {
		StageSettings stage = GetStage();
		if (useHiscore) {
			stars = StageSettings.GetStars(stage, NetworkManager.GetManager().Player);
			trackedValue = LevelSettings.GetHiscore(stage.level, NetworkManager.GetManager().Player);
		}
		string[] levelStats = new string[2];
		levelStats[0] = stars + " / " + stage.totalCards * ConstantHolder.maxStars;
		levelStats[1] = trackedValue + " / " + Mathf.FloorToInt(stage.level.medalValuePerRound * Mathf.CeilToInt(stage.totalCards / (float)stage.cardPerRound));
		return levelStats;
	}

	public StageSettings GetStage() {
		return planets[currentPlanetIndex].stages[currentStageIndex];
	}

	public bool[] GetPlanetClears() {
		bool[] clears = new bool[planets.Length];
		for(int i = 0; i < clears.Length; ++i) {
			clears[i] = GetFirstUnclear(i) == planets[i].stages.Length;
			if (clears[i] == false)
				break;
		}
		return clears;
	}

	public int GetCurrentFirstUnclear() {
		return GetFirstUnclear(currentPlanetIndex);
	}

	int GetFirstUnclear(int index) {
		return planets[index].GetFirstUnclear();
	}

	public Sprite GetCurrentPlanetBackground() {
		return planets[currentPlanetIndex].stageHubBackground;
	}
	
	public bool GetStageMedal(int index) {
		return LevelSettings.GetMedal(planets[currentPlanetIndex].stages[index].level, NetworkManager.GetManager().Player);
	}
}
