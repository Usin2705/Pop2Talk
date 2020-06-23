using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMaster : MonoBehaviour {

	[SerializeField] bool reset;
	public bool skipTransitions;
	public bool skipWords;
	public bool skipPops;

	public static DebugMaster Instance {
		get; private set;
	}

	private void Awake() {
		Instance = this;
	}

	private void OnValidate() {
		if (reset) {
			reset = false;
			skipTransitions = false;
			skipWords = false;
			skipPops = false;
		}
	}
}
