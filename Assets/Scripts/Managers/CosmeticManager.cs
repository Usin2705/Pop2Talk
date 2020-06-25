using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CosmeticManager : MonoBehaviour {


	[SerializeField] Cosmetic[] allCosmetics;

	Dictionary<string, Cosmetic> cosmetics = new Dictionary<string, Cosmetic>();
	HashSet<string> unlockedCosmetics = new HashSet<string>();
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
		return FindUnregisteredCosmetic(id);
	}

	public Cosmetic GetEquippedCosmetic(CosmeticSlot slot) {
		if (equippedCosmetics.ContainsKey(slot))
			return GetCosmetic(equippedCosmetics[slot]);
		return null;
	}

	Cosmetic FindUnregisteredCosmetic(string id) {
		foreach (Cosmetic c in allCosmetics) {
			if (c.Id != id)
				continue;
			cosmetics.Add(id, c);
			return c;
		}
		return null;
	}

	public void EquipCosmetic(string[] ids) {
		for (int i = 0; i < ids.Length; ++i) {
			EquipCosmetic(ids[i]);
		}
	}
	
	public void EquipCosmetic(string id) {
		bool slotChanged = true;
		if (!unlockedCosmetics.Contains(id))
			UnlockCosmetic(id);
		Cosmetic c = GetCosmetic(id);
		if (equippedCosmetics.ContainsKey(c.slot)) {
			if (id != equippedCosmetics[c.slot])
				equippedCosmetics[c.slot] = id;
			else
				slotChanged = false;
		} else
			equippedCosmetics.Add(c.slot, id);

		if (slotChanged) {
			FakeServerManager.GetManager().EquipCosmetic(id, c.slot);
		}
	}

	public Sprite UnlockCosmeticFromBox(LootBoxSettings box) {
		string id = box.cosmeticIds.GetRandom();
		UnlockCosmetic(id);
		return GetCosmetic(id).sprite;
	}

	public void UnlockCosmetic(string[] ids) {
		foreach (string s in ids)
			UnlockCosmetic(s);
	}

	public void UnlockCosmetic(string id) {
		if (!unlockedCosmetics.Contains(id)) {
			FakeServerManager.GetManager().UnlockCosmetic(id);
			unlockedCosmetics.Add(id);
		}
	}

	public Cosmetic[] GetUnlockedCosmetics() {
		Cosmetic[] cosmetics = new Cosmetic[unlockedCosmetics.Count];
		int i = 0;
		foreach (string s in unlockedCosmetics) {
			cosmetics[i] = GetCosmetic(s);
			++i;
		}
		return cosmetics;
	}

}
