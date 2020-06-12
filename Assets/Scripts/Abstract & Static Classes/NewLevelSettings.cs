using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NewLevelSettings : ScriptableObject {

	public bool memoryOnly;
	public int minWords;
	public int maxWords;
	[Space]
	public bool spacedustAffectsCoins;
	public float spacedustMean;
	public float spacedustMaxDeviation;

}