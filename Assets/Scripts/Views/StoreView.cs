using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreView : View {

	[SerializeField] View shipHubView = null;
	[SerializeField] View collectionView = null;
	[SerializeField] UIButton backButton = null;
	[SerializeField] UIButton showCollectionButton = null;
	[SerializeField] Text coinText = null;
	[Space]
	[SerializeField] RectTransform lootHolder = null;


	List<LootBoxSettings> purchasables = new List<LootBoxSettings>();
	Dictionary<Cosmetic, UIButton> collectedButtons = new Dictionary<Cosmetic, UIButton>();
	Dictionary<Cosmetic, UIButton> equippedButtons = new Dictionary<Cosmetic, UIButton>();

	List<StoreButton> lootboxButtons = new List<StoreButton>();

	float shakeDuration = 1;

	protected override void Initialize() {
		base.Initialize();
		backButton.SubscribePress(Back);
		showCollectionButton.SubscribePress(ShowCollections);
	}

	public override void Activate() {
		base.Activate();
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

	void ShowCollections() {
		ViewManager.GetManager().ShowView(collectionView);
	}

	IEnumerator PurchaseRoutine(int index) {
		InputManager.GetManager().SendingInputs = false;
		CurrencyMaster.Instance.ModifyCoins(-StoreManager.GetManager().GetBoxes()[index].price);
		coinText.text = CurrencyMaster.Instance.Coins.ToString();
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
		Cosmetic c = CosmeticManager.GetManager().UnlockCosmeticFromBox(StoreManager.GetManager().GetBoxes()[index]);

		if (c.slot == CosmeticSlot.ShipBottom)
			UnlockOverlay.Instance.ShowBotUnlock(sortingOrder, c.icon, null);
		else if (c.slot == CosmeticSlot.ShipTop)
			UnlockOverlay.Instance.ShowTopUnlock(sortingOrder, c.icon, null);
		else
			UnlockOverlay.Instance.ShowUnlock(sortingOrder, c.icon, null);
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
