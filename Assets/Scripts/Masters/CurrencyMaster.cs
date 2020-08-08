using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyMaster : MonoBehaviour {

	static CurrencyMaster instance;

	public int Coins { get; protected set; }

	public int LootLevel { get; set; }

	public float minGamesToChest = 3;
	public float maxGamesToChest = 8;

	public static CurrencyMaster Instance {
		get {
			if (instance == null) {
				instance = new GameObject("Currency Master").AddComponent<CurrencyMaster>();
			}

			return instance;
		}
	}

	public int IncreaseCoins(float starRatio, float dustRatio) {
		if (starRatio == 0 || dustRatio == 0)
			return 0;
		int addedCoins = Mathf.RoundToInt(1f / Mathf.Lerp(maxGamesToChest, minGamesToChest, starRatio * dustRatio) * GetLootLevelCoins(LootLevel));
		ModifyCoins(addedCoins);
		return addedCoins;
	}

	public void ModifyCoins(int amount) {
		if (amount == 0)
			return;
		Coins += amount;
		NetworkManager.GetManager().UpdateCoins(Coins);
	}

	public void SetCoins(int coins) {
		Coins = coins;
	}

	public int GetLootLevelCoins(int level) {
		return StoreManager.GetManager().GetBoxPrice(level);
	}
}
