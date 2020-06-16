using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreManager : MonoBehaviour {

	static StoreManager sm;

	[SerializeField] LootBoxSettings[] boxes;


	public static StoreManager GetManager() {
		return sm;
	}

	void Awake() {
		sm = this;
	}

	public int GetBoxPrice(int index) {
		return boxes[index].price;
	}

}
