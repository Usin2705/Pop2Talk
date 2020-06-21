using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeServerManager : MonoBehaviour {

	[SerializeField] int characterIndex = -1;
	[SerializeField] int coins = 100;
	[SerializeField] StringInt[] words;
	[SerializeField] string equipId;
	[SerializeField] string[] collectionId;
	[SerializeField] int largestModuleIndex;
	[SerializeField] string[] cardWords;
	[SerializeField] WordCardType[] cardTypes;

	static FakeServerManager fsm;

	[System.Serializable]
	struct StringInt {
		public int stars;
		public string word;
	}

	void Awake() {
		fsm = this;
	}

	public static FakeServerManager GetManager() {
		return fsm;
	}

	public void Connect() {
		CharacterManager.GetManager().SetCharacter(characterIndex);

		string[] strings = new string[words.Length];
		int[] stars = new int[words.Length];
		for (int i = 0; i < stars.Length; ++i) {
			strings[i] = words[i].word;
			stars[i] = words[i].stars;
		}
		WordMaster.Instance.SetStarAmounts(strings, stars);
		CurrencyMaster.Instance.Coins = coins;
		CosmeticManager.GetManager().UnlockCosmetic(collectionId);
		CosmeticManager.GetManager().EquipCosmetic(equipId);
	}

	public void UpdateCoins(int newCoins) {

	}

	public void EquipCosmetic(string id, CosmeticSlot slot) {

	}

	public void UnlockCosmetic(string id) {

	}

	public int GetLargestModuleIndex() {
		return largestModuleIndex;
	}

	public string[] GetCardWords() {
		return cardWords;
	}

	public WordCardType[] GetCardTypes() {
		return cardTypes;
	}

	public float GetCardPar() {
		return 2;
	}
}
