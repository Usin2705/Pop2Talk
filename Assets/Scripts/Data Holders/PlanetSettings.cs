using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Planet Settings")]
public class PlanetSettings : ScriptableObject {
	public Sprite planetSprite;
	public Sprite stageHubBackground;
	public GameObject nodeLayoutPrefab;

	public StageSettings[] stages;

	public int GetFirstUnclear() {
		for (int i = 0; i < stages.Length; ++i) {
			if (!StageSettings.GetDoneStatus(stages[i], NetworkManager.GetManager().Player)) 
				return i;
		}
		return stages.Length;
	}

}
