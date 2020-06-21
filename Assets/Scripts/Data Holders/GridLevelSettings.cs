using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Grid Level Settings", menuName = "CustomObjects/Grid Level Settings")]
public class GridLevelSettings : LevelSettings {

	public GridGameMode gameMode;
	public LevelTypeSettings[] typeSettings;

}