using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyMaster : MonoBehaviour {

	static CurrencyMaster instance;

	public int Coins { get; set; }

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
		Coins += addedCoins;
		FakeServerManager.GetManager().UpdateCoins(Coins);
		return addedCoins;
	}

	public int GetLootLevelCoins(int level) {
		return StoreManager.GetManager().GetBoxPrice(level);
	}
}
