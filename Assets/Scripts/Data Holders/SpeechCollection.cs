using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Speech Collection")]
public class SpeechCollection : ScriptableObject {

	[System.Serializable]
	public struct Speech {
		public string speech;
		public float pause;
	}

	public List<Speech> speeches;
	public bool hideAtEnd;
}
