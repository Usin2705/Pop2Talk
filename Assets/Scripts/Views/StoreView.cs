using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreView : View {

	[SerializeField] View shipHubView;
	[SerializeField] UIButton backButton;
	[SerializeField] UIButton showCollectionButton;
	[SerializeField] UIButton showLootButton;
	[SerializeField] Text coinText;
	[SerializeField] Image equippedImage;
	[SerializeField] Image[] shipImages;
	[Space]
	[SerializeField] GameObject unlockScreen;
	[SerializeField] Image unlockImage;
	[SerializeField] Image whiteCurtain;
	[SerializeField] UIButton closeUnlockButton;
	[Space]
	[SerializeField] GameObject lootScreen;
	[SerializeField] RectTransform lootHolder;
	[SerializeField] GameObject storeButtonPrefab;
	[Space]
	[SerializeField] GameObject collectionScreen;
	[SerializeField] GridPageHandler collectionGridPage;
	[SerializeField] GameObject selectableButtonPrefab;
	[SerializeField] UIButton[] collectionButtons;


	List<LootBoxSettings> purchasables = new List<LootBoxSettings>();
	Dictionary<Cosmetic, UIButton> collectedButtons = new Dictionary<Cosmetic, UIButton>();
	Dictionary<Cosmetic, UIButton> equippedButtons = new Dictionary<Cosmetic, UIButton>();

	List<StoreButton> lootboxButtons = new List<StoreButton>();

	float shakeDuration = 1;

	int chosenCollectionIndex = 0;

	protected override void Initialize() {
		base.Initialize();
		backButton.SubscribePress(Back);
		showCollectionButton.SubscribePress(ShowCollections);
		showLootButton.SubscribePress(ShowLoot);
		closeUnlockButton.SubscribePress(CloseUnlock);
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
			UIButton button = Instantiate(selectableButtonPrefab, collectionGridPage.GetNextParent((int)cosmetics[i].slot)).GetComponent<UIButton>();
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
		collectionScreen.SetActive(true);
		lootScreen.SetActive(false);
	}

	void ShowCollection(int index) {
		collectionButtons[chosenCollectionIndex].Deselect();
		chosenCollectionIndex = index;
		collectionGridPage.ShowCollection(chosenCollectionIndex);

		if (index == (int)CosmeticSlot.ShipTop || index == (int)CosmeticSlot.ShipMid ||index == (int)CosmeticSlot.ShipBottom) {
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
				if (index == (int)c.slot) {
					equippedImage.sprite = c.icon;
					found = true;
					break;
				}
			}
			equippedImage.gameObject.SetActive(found);
		}
	}

	void ShowLoot() {
		collectionScreen.SetActive(false);
		lootScreen.SetActive(true);
	}

	public override void Activate() {
		base.Activate();
		ShowLoot();
		coinText.text = CurrencyMaster.Instance.Coins.ToString();
		LootBoxSettings[] settings = StoreManager.GetManager().GetBoxes();
		float ratio = WordMaster.Instance.GetBestResults().Count / (float)WordMaster.Instance.TotalWords;
		for (int i = purchasables.Count; i < settings.Length; ++i) {
			if (i / (float)settings.Length > ratio)
				break;

			purchasables.Add(settings[i]);
			StoreButton button = lootHolder.GetChild(i).GetComponent<StoreButton>();
			button.SetUp(settings[i], i, LootClicked);
			lootboxButtons.Add(button);
		}
	}

	void LootClicked(int index) {
		if (CurrencyMaster.Instance.Coins >= purchasables[index].price) {
			StartCoroutine(PurchaseRoutine(index));
		}
	}

	IEnumerator PurchaseRoutine(int index) {
		InputManager.GetManager().SendingInputs = false;
		if (!DebugMaster.Instance.skipTransitions) {
			Transform box = lootboxButtons[index].GetBox();
			Vector3 start = box.transform.position;
			float a = 0, max = 30 * 1080f / Screen.width;
			//lootboxButtons[index].TogglePrice(false);
			while (a < 1) {
				a += Time.deltaTime / shakeDuration;
				box.position = start + (Vector3)Random.insideUnitCircle * max;
				yield return null;
			}
			box.transform.position = start;
			//lootboxButtons[index].TogglePrice(true);
		}

		CurrencyMaster.Instance.ModifyCoins(-StoreManager.GetManager().GetBoxes()[index].price);
		coinText.text = CurrencyMaster.Instance.Coins.ToString();
		unlockScreen.SetActive(true);
		unlockImage.sprite = CosmeticManager.GetManager().UnlockCosmeticFromBox(StoreManager.GetManager().GetBoxes()[index]);
		if (!DebugMaster.Instance.skipTransitions) {
			whiteCurtain.gameObject.SetActive(true);
			yield return new WaitForSeconds(0.5f);
			float a = 0;
			while (a < 1) {
				a += Time.deltaTime / 0.5f;
				whiteCurtain.color = new Color(1, 1, 1, 1 - a);
				yield return null;
			}
		}
		whiteCurtain.gameObject.SetActive(false);
		whiteCurtain.color = Color.white;
		InputManager.GetManager().SendingInputs = true;
	}

	void CloseUnlock() {
		unlockScreen.SetActive(false);
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
		equippedImage.sprite = CosmeticManager.GetManager().GetEquippedCosmetic(CosmeticSlot.ShipTop).sprite;
	}

	void Back() {
		ViewManager.GetManager().ShowView(shipHubView);
	}

	public override UIButton GetPointedButton() {
		return null;
	}

	public override UIButton[] GetAllButtons() {
		return null;
	}
}
