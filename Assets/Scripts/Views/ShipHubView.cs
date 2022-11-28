using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipHubView : View {
	
	[SerializeField] View storeView = null;
	[SerializeField] View wordPearlView = null;
	[SerializeField] View travelView = null;

	[Space]
	[SerializeField] UIButton backButton = null;
	[SerializeField] UIButton travelButton = null;
	[SerializeField] UIButton storeButton = null;
	[SerializeField] UIButton pearlButton = null;
	[SerializeField] Text coins = null;
	[SerializeField] Text pearls = null;
	[SerializeField] Image character = null;
	[SerializeField] Image backWall = null;
	[SerializeField] Image backFloor = null;
	[SerializeField] Image controlPanel = null;


	protected override void Initialize() {
		base.Initialize();
		backButton.SubscribePress(Back);
		travelButton.SubscribePress(GotoTravelView);
		storeButton.SubscribePress(GotoStoreView);
		pearlButton.SubscribePress(GotoPearlView);

		// TODO 
		// We temporarily remove all those function for testing purpose
		backButton.gameObject.SetActive(false);
		storeButton.gameObject.SetActive(false);
		pearlButton.gameObject.SetActive(false);

	}

	public override void Activate() {
		base.Activate();
		coins.text = CurrencyMaster.Instance.Coins.ToString();
		pearls.text = WordMaster.Instance.GetBestResults().Count.ToString() + "/" + (WordMaster.Instance.TotalWords);
		character.sprite = CharacterManager.GetManager().CurrentCharacter.characterSprite;
		Cosmetic wall = CosmeticManager.GetManager().GetEquippedCosmetic(CosmeticSlot.Wallpaper);
		backWall.sprite = (wall != null) ? wall.sprite : null;
		backFloor.sprite = (wall != null) ? wall.extraSprites[0] : null;
		controlPanel.sprite = (wall != null) ? wall.extraSprites[1] : null;
	}

	public override UIButton[] GetAllButtons() {
		UIButton[] uiButtons = new UIButton[4];
		uiButtons[0] = backButton;
		uiButtons[1] = travelButton;
		uiButtons[2] = storeButton;
		uiButtons[3] = pearlButton;
		return uiButtons;
	}

	void GotoTravelView() {
		ViewManager.GetManager().ShowView(travelView);
	}

	void GotoStoreView() {
		ViewManager.GetManager().ShowView(storeView);
	}

	void GotoPearlView() {
		ViewManager.GetManager().ShowView(wordPearlView);
	}

	public override void Back() {
		base.Back();
		Exit();
	}

	void Exit() {
		NetworkManager.GetManager().ControlledExit();
		Application.Quit();
	}

	public override UIButton GetPointedButton() {
		return travelButton;
	}
}
