using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CosmeticManager : MonoBehaviour {


	[SerializeField] Cosmetic[] allCosmetics;

	Dictionary<string, Cosmetic> cosmetics = new Dictionary<string, Cosmetic>();
	HashSet<string> unclockedCosmetics = new HashSet<string>();
	Dictionary<CosmeticSlot, string> equippedCosmetics = new Dictionary<CosmeticSlot, string>();

	static CosmeticManager cm;

	public static CosmeticManager GetManager() {
		return cm;
	}

	void Awake() {
		cm = this;
		foreach (Cosmetic c in allCosmetics) {
			cosmetics.Add(c.Id, c);
		}
	}

	public Cosmetic GetCosmetic(string id) {
		if (cosmetics.ContainsKey(id))
			return cosmetics[id];
		return FindCosmetic(id);
	}

	public Cosmetic GetEquippedCosmetic(CosmeticSlot slot) {
		if (equippedCosmetics.ContainsKey(slot))
			return FindCosmetic(equippedCosmetics[slot]);
		return null;
	}

	Cosmetic FindCosmetic(string id) {
		foreach (Cosmetic c in allCosmetics) {
			if (c.Id != id)
				continue;
			cosmetics.Add(id, c);
			return c;
		}
		return null;
	}

	public void EquipCosmetic(CosmeticSlot[] slots, string[] ids) {
		for (int i = 0; i < slots.Length; ++i) {
			EquipCosmetic(slots[i], ids[i]);
		}
	}
	
	public void EquipCosmetic(CosmeticSlot slot, string id) {
		bool slotChanged = true;
		if (equippedCosmetics.ContainsKey(slot)) {
			if (id != equippedCosmetics[slot])
				equippedCosmetics[slot] = id;
			else
				slotChanged = false;
		} else
			equippedCosmetics.Add(slot, id);

		if (slotChanged) {

		}
	}

}
