using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugMaster : MonoBehaviour {

	[SerializeField] bool reset = false;
	[SerializeField] GameObject debugScreen = null;
	[SerializeField] Text debugText = null;

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

	public void DebugText(string text) {
		debugText.text += text  +"\n";
		debugScreen.SetActive(true);
	}

	public void CloseDebug() {
		debugScreen.SetActive(false);
	}
}
