using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCatcher : MonoBehaviour {

	CatchModeHandler catchModeHandler;

	public void SetCatchModeHandler(CatchModeHandler catchModeHandler) {
		this.catchModeHandler = catchModeHandler;
	}

	void OnTriggerEnter(Collider other) {
		Debug.Log("yeya");
		MatchableTile mt = other.GetComponentInParent<MatchableTile>();
		if (mt != null && catchModeHandler != null) {
			catchModeHandler.CatchTile(mt);
		}
	}

}
