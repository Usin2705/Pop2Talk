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
	[SerializeField] RectTransform collectionHolder;
	[SerializeField] GameObject selectableButtonPrefab;


	List<LootBoxSettings> purchasables = new List<LootBoxSettings>();
	Dictionary<string, UIButton> collecteds = new Dictionary<string, UIButton>();

	List<UIButton> collectedButtons = new List<UIButton>();

	UIButton chosen;
	
	protected override void Initialize() {
		base.Initialize();
		backButton.SubscribePress(Back);
		showCollectionButton.SubscribePress(ShowCollection);
		showLootButton.SubscribePress(ShowLoot);
	}

	void ShowCollection() {
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
			StoreButton button = Instantiate(storeButtonPrefab, lootHolder).GetComponent<StoreButton>();
			button.SetUp(settings[i], i, LootClicked);
		}
		string equippedId = CosmeticManager.GetManager().GetEquippedCosmetic(CosmeticSlot.Ship).Id;
		Cosmetic[] cosmetics = CosmeticManager.GetManager().GetUnlockedCosmetics();
		for (int i = 0; i < cosmetics.Length; ++i) {
			string s = cosmetics[i].Id;
			if (collecteds.ContainsKey(s))
				continue;
			UIButton button = Instantiate(selectableButtonPrefab, collectionHolder).GetComponent<UIButton>();
			collecteds.Add(s, button);
			if (s == equippedId) {
				button.Press();
				chosen = button;
			}
			button.SubscribePress(() => { CollectionClicked(s); });
			button.SetSprite(CosmeticManager.GetManager().GetCosmetic(s).icon);
		}
	}

	void LootClicked(int index) {
		Debug.Log("yeya");
		if (CurrencyMaster.Instance.Coins >= purchasables[index].price) {
			CurrencyMaster.Instance.Coins -= purchasables[index].price;
			coinText.text = CurrencyMaster.Instance.Coins.ToString();
		}
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
