using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level Settings", menuName = "CustomObjects/Grid Level Settings")]
public class LevelSettings : ScriptableObject{

	public bool memoryOnly;
	public int minWords;
	public int maxWords;
	[Space]
	public bool spacedustAffectsCoins;
	public float spaceDustMin;
	public float spaceDustMax;

	public GameMode gameMode;
	public LevelTypeSettings[] typeSettings;

}