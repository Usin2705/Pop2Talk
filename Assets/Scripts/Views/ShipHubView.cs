﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipHubView : View {
	
	[SerializeField] View storeView;
	[SerializeField] View wordPearlView;
	[SerializeField] View travelView;

	[Space]
	[SerializeField] UIButton backButton;
	[SerializeField] UIButton travelButton;
	[SerializeField] UIButton storeButton;
	[SerializeField] UIButton pearlButton;
	[SerializeField] Text coins;
	[SerializeField] Text pearls;
	[SerializeField] Image character;

	protected override void Initialize() {
		base.Initialize();
		backButton.SubscribePress(Exit);
		travelButton.SubscribePress(GotoTravelView);
		storeButton.SubscribePress(GotoStoreView);
		pearlButton.SubscribePress(GotoPearlView);

	}

	public override void Activate() {
		base.Activate();
		coins.text = CurrencyMaster.Instance.Coins.ToString();
		pearls.text = WordMaster.Instance.GetBestResults().Count.ToString() + "/" + (WordMaster.Instance.GetBestResults().Count + WordMaster.Instance.GetUnsaidWordCount()).ToString();
		character.sprite = CharacterManager.GetManager().CurrentCharacter.characterSprite;
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

	void Exit() {
		NetworkManager.GetManager().ControlledExit();
		Application.Quit();
	}

	public override UIButton GetPointedButton() {
		return travelButton;
	}
}
