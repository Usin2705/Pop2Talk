using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionView : View {

	[SerializeField] View shipHubView = null;
	[SerializeField] View storeView = null;
	[SerializeField] UIButton backButton = null;
	[SerializeField] UIButton showStoreButton = null;
	[SerializeField] Image equippedImage = null;
	[SerializeField] Image[] shipImages = null;
	[Space]
	[SerializeField] GridPageHandler collectionGridPage = null;
	[SerializeField] GameObject selectableButtonPrefab = null;
	[SerializeField] UIButton[] collectionButtons = null;
	[SerializeField] GameObject bottomButtonPrefab = null;
	[SerializeField] GameObject midButtonPrefab = null;
	[SerializeField] GameObject topButtonPrefab = null;


	List<LootBoxSettings> purchasables = new List<LootBoxSettings>();
	Dictionary<Cosmetic, UIButton> collectedButtons = new Dictionary<Cosmetic, UIButton>();
	Dictionary<Cosmetic, UIButton> equippedButtons = new Dictionary<Cosmetic, UIButton>();

	List<StoreButton> lootboxButtons = new List<StoreButton>();

	int chosenCollectionIndex = 0;

	protected override void Initialize() {
		base.Initialize();
		backButton.SubscribePress(Back);
		showStoreButton.SubscribePress(ShowStore);
		collectionButtons[chosenCollectionIndex].Press();
		for (int i = 0; i < collectionButtons.Length; ++i) {
			int j = i;
			collectionButtons[i].SubscribeSelect(() => { ShowCollection(j); });
		}
	}

	void ShowCollections() {
		Dictionary<CosmeticSlot, string> equippedIDs = CosmeticManager.GetManager().GetEquippedIDs();
		equippedButtons = new Dictionary<Cosmetic, UIButton>();

		Cosmetic[] cosmetics = CosmeticManager.GetManager().GetUnlockedCosmetics();
		for (int i = 0; i < cosmetics.Length; ++i) {
			if (collectedButtons.ContainsKey(cosmetics[i]))
				continue;
			GameObject prefab = selectableButtonPrefab;
			if (cosmetics[i].slot == CosmeticSlot.ShipTop)
				prefab = topButtonPrefab;
			else if (cosmetics[i].slot == CosmeticSlot.ShipMid)
				prefab = midButtonPrefab;
			else if (cosmetics[i].slot == CosmeticSlot.ShipBottom)
				prefab = bottomButtonPrefab;

			UIButton button = Instantiate(prefab, collectionGridPage.GetNextParent((int)cosmetics[i].slot)).GetComponent<UIButton>();
				collectedButtons.Add(cosmetics[i], button);
			if (equippedIDs[cosmetics[i].slot] == cosmetics[i].Id) {
				button.Press();
				equippedButtons.Add(cosmetics[i], button);
				if (cosmetics[i].slot == (CosmeticSlot)chosenCollectionIndex)
					equippedImage.sprite = cosmetics[i].sprite;
			}
			int j = i;
			button.SubscribePress(() => { CollectionClicked(cosmetics[j]); });
			button.SetSprite(cosmetics[i].icon);
			button.canPressSelected = false;
		}

		collectionGridPage.ShowCollection(chosenCollectionIndex);
		UpdateEquippedImages();
	}

	void ShowCollection(int index) {
		collectionButtons[chosenCollectionIndex].Deselect();
		chosenCollectionIndex = index;
		collectionGridPage.ShowCollection(chosenCollectionIndex);

		UpdateEquippedImages();
	}

	void UpdateEquippedImages() {
		if (chosenCollectionIndex == (int)CosmeticSlot.ShipTop || chosenCollectionIndex == (int)CosmeticSlot.ShipMid || chosenCollectionIndex == (int)CosmeticSlot.ShipBottom) {
			equippedImage.gameObject.SetActive(false);

			for (int i = 0; i < shipImages.Length; ++i) {
				shipImages[i].gameObject.SetActive(true);
				foreach (Cosmetic c in equippedButtons.Keys) {
					if (i == (int)c.slot) {
						shipImages[i].sprite = c.icon;
						break;
					}
				}
			}
		} else {
			bool found = false;
			foreach (Cosmetic c in equippedButtons.Keys) {
				if (chosenCollectionIndex == (int)c.slot) {
					equippedImage.sprite = c.icon;
					found = true;
					break;
				}
			}
			for (int i = 0; i < shipImages.Length; ++i) {
				shipImages[i].gameObject.SetActive(false);
			}
			equippedImage.gameObject.SetActive(found);
		}
	}

	void ShowStore() {
		ViewManager.GetManager().ShowView(storeView);
	}

	public override void Activate() {
		base.Activate();
		ShowCollections();
	}

	void CollectionClicked(Cosmetic cosmetic) {
		if (equippedButtons.ContainsKey(cosmetic) || cosmetic == null)
			return;
		Cosmetic current = null;
		foreach (Cosmetic c in equippedButtons.Keys) {
			if (c.slot == cosmetic.slot) {
				current = c;
				break;
			}
		}
		equippedButtons[current].Deselect();
		equippedButtons.Remove(current);
		equippedButtons.Add(cosmetic, collectedButtons[cosmetic]);
		CosmeticManager.GetManager().EquipCosmetic(cosmetic.Id);
		UpdateEquippedImages();
	}

	public override void Back() {
		base.Back();
		ViewManager.GetManager().ShowView(shipHubView);
	}

	public override UIButton GetPointedButton() {
		return null;
	}

	public override UIButton[] GetAllButtons() {
		return null;
	}
}
