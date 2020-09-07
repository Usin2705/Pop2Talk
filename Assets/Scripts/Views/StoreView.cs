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
	[SerializeField] Image shipImage;
	[Space]
	[SerializeField] GameObject lootScreen;
	[SerializeField] RectTransform lootHolder;
	[SerializeField] GameObject storeButtonPrefab;
	[Space]
	[SerializeField] GameObject collectionScreen;
	[SerializeField] GridPageHandler collectionGridPage;
	[SerializeField] GameObject selectableButtonPrefab;


	List<LootBoxSettings> purchasables = new List<LootBoxSettings>();
	Dictionary<string, UIButton> collecteds = new Dictionary<string, UIButton>();

	List<StoreButton> lootboxButtons = new List<StoreButton>();
	List<UIButton> collectedButtons = new List<UIButton>();

	UIButton chosen;

	float shakeDuration = 1;

	protected override void Initialize() {
		base.Initialize();
		backButton.SubscribePress(Back);
		showCollectionButton.SubscribePress(ShowCollection);
		showLootButton.SubscribePress(ShowLoot);
	}

	void ShowCollection() {
		string equippedId = CosmeticManager.GetManager().GetEquippedCosmetic(CosmeticSlot.Ship).Id;
		Cosmetic[] cosmetics = CosmeticManager.GetManager().GetUnlockedCosmetics();
		for (int i = 0; i < cosmetics.Length; ++i) {
			string s = cosmetics[i].Id;
			if (collecteds.ContainsKey(s))
				continue;
			UIButton button = Instantiate(selectableButtonPrefab, collectionGridPage.GetParent()).GetComponent<UIButton>();
			collecteds.Add(s, button);
			if (s == equippedId) {
				button.Press();
				chosen = button;
			}
			button.SubscribePress(() => { CollectionClicked(s); });
			button.SetSprite(CosmeticManager.GetManager().GetCosmetic(s).icon);
		}

		collectionScreen.SetActive(true);
		lootScreen.SetActive(false);
	}

	void ShowLoot() {
		collectionScreen.SetActive(false);
		lootScreen.SetActive(true);
	}

	public override void Activate() {
		base.Activate();
		ShowLoot();
		coinText.text = CurrencyMaster.Instance.Coins.ToString();
		shipImage.sprite = CosmeticManager.GetManager().GetEquippedCosmetic(CosmeticSlot.Ship).sprite;
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
		CurrencyMaster.Instance.ModifyCoins(-StoreManager.GetManager().GetBoxes()[index].price);
		coinText.text = CurrencyMaster.Instance.Coins.ToString();
		if (!DebugMaster.Instance.skipTransitions) {
			Transform box = lootboxButtons[index].GetBox();
			Vector3 start = box.transform.position;
			float a = 0, max = 30 * 1080f/Screen.width;
			//lootboxButtons[index].TogglePrice(false);
			while (a < 1) {
				a += Time.deltaTime / shakeDuration;
				box.position = start + (Vector3)Random.insideUnitCircle * max;
				yield return null;
			}
			box.transform.position = start;
			//lootboxButtons[index].TogglePrice(true);
		}
		
		UnlockOverlay.Instance.ShowUnlock(sortingOrder, CosmeticManager.GetManager().UnlockCosmeticFromBox(StoreManager.GetManager().GetBoxes()[index]), null);
	}
	
	void CollectionClicked(string id) {
		if (chosen == collecteds[id])
			return;
		if (chosen != null) {
			chosen.Deselect();
		}
		chosen = collecteds[id];
		CosmeticManager.GetManager().EquipCosmetic(id);
		shipImage.sprite = CosmeticManager.GetManager().GetEquippedCosmetic(CosmeticSlot.Ship).sprite;
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
