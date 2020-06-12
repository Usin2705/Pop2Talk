using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyMaster : MonoBehaviour {

	static CurrencyMaster instance;

	public int Coins { get; set; }

	public int LootLevel { get; set; }

	public int gamesToChest = 3;

	public static CurrencyMaster Instance {
		get {
			if (instance == null) {
				instance = new GameObject("Currency Master").AddComponent<CurrencyMaster>();
			}

			return instance;
		}
	}

	public int IncreaseCoins(float starRatio, float dustRatio) {
		return 0;
	}
}
