using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Word Data", menuName = "CustomObjects/Word Data")]
public class WordData : ScriptableObject {

	public Sprite picture;
	public Language language;
	public string spelling;
	public AudioClip[] pronunciations;
}
