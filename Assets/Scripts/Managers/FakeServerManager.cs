using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeServerManager : MonoBehaviour {

	
	[SerializeField] string equipId;
	[SerializeField] string[] collectionId;

	static FakeServerManager fsm;
	
	void Awake() {
		fsm = this;
	}

	public static FakeServerManager GetManager() {
		return fsm;
	}

	public void Connect() {
		//CharacterManager.GetManager().SetCharacter(characterIndex);
		/*
		string[] strings = new string[words.Length];
		int[] stars = new int[words.Length];
		for (int i = 0; i < stars.Length; ++i) {
			strings[i] = words[i].word;
			stars[i] = words[i].stars;
		}*/
		//CurrencyMaster.Instance.SetCoins(coins);
		CosmeticManager.GetManager().UnlockCosmetic(collectionId);
		CosmeticManager.GetManager().EquipCosmetic(equipId);
	}
	public void EquipCosmetic(string id, CosmeticSlot slot) {

	}

	public void UnlockCosmetic(string id) {

	}
}
