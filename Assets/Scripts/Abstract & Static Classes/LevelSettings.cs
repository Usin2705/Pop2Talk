using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LevelSettings : ScriptableObject {

	public bool memoryOnly;
	public int minWords;
	public int maxWords;
	[Space]
	public bool spacedustAffectsCoins;
	public float spaceDustMin;
	public float spaceDustMax;

}