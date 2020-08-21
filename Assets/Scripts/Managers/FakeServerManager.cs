using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeServerManager : MonoBehaviour {

	static FakeServerManager fsm;
	
	void Awake() {
		fsm = this;
	}

	public static FakeServerManager GetManager() {
		return fsm;
	}

	public void Connect() {
		CosmeticManager.GetManager().UnlockAndEquipDefaults();
	}

	public void EquipCosmetic(string id, CosmeticSlot slot) {

	}

	public void UnlockCosmetic(string id) {

	}
}
