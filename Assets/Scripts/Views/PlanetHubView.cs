using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetHubView : View {

	[SerializeField] View characterSelect;
	[SerializeField] View stageHub;
	[SerializeField] View wordPearl;
	[Space]
	[SerializeField] UIButton backButton;
	[SerializeField] UIButton pearlButton;
	[SerializeField] GameObject planetButtonPrefab;
	[SerializeField] Transform buttonRoot;
	[Space]
	[SerializeField] Transform shipStart;
	[SerializeField] Transform shipStop;

	int highestOpen;

	List<UIButton> buttons = new List<UIButton>();
	int chosenIndex;

	bool overlayActive;

	protected override void Initialize() {
		base.Initialize();

		Sprite[] sprites = PlanetManager.GetManager().GetPlanetSprites();
		UIButton button;
		for (int i = 0; i < sprites.Length; ++i) {
			button = Instantiate(planetButtonPrefab, buttonRoot.GetChild(buttonRoot.childCount - 1 - i)).GetComponent<UIButton>();
			button.SetSprite(sprites[i]);
			int j = i; // If subscribed simply to i, the lambda apparentaly keeps a reference to it and makes all subscriptions equal to sprites.Length
			button.SubscribePress(() => { PlanetPressed(j); });
			buttons.Add(button);
		}
		backButton.SubscribePress(Back);
		pearlButton.SubscribePress(GotoWordPearlView);
        buttons.Add(backButton);
        buttons.Add(pearlButton);
	}

	public override void Activate() {
		base.Activate();
		bool[] clears = PlanetManager.GetManager().GetPlanetClears();
		for (int i = 1; i < clears.Length; ++i) {
			bool locked = !clears[i - 1];
			if (!locked)
				highestOpen = i;
			buttons[i].SetLocked(locked);
		}
	}

	void PlanetPressed(int index) {
		chosenIndex = index;
		PlanetManager.GetManager().SetPlanet(index);
		string[] stats = PlanetManager.GetManager().GetPlanetStats();
		Sprite[] icons = new Sprite[2];
		if (IconManager.GetManager() != null) {
			icons[0] = IconManager.GetManager().starIcon;
			icons[1] = IconManager.GetManager().medalIcon;
		}
		Sprite[] buttonIcons = new Sprite[1];
		if (IconManager.GetManager() != null) {
			buttonIcons[0] = IconManager.GetManager().arrowIcon;
		}
		Callback[] callbacks = new Callback[1] { GotoPlanet };
		DataOverlayManager.GetManager().Show(sortingOrder, true, "", icons, stats, callbacks, buttonIcons, null);
	}

	void GotoPlanet() {
		doExitFluff = true;
		ViewManager.GetManager().ShowView(stageHub);
	}

	void Back() {
		doExitFluff = false;
		ViewManager.GetManager().ShowView(characterSelect);
	}

	void GotoWordPearlView() {
		doExitFluff = false;
		ViewManager.GetManager().ShowView(wordPearl);
	}

	public override void ExitFluff(Callback Done) {
		Vector3[] positions = new Vector3[] { shipStart.position, shipStop.position, buttons[chosenIndex].transform.position };
		Vector3[] sizes = new Vector3[] { Vector3.one, Vector3.one, Vector3.zero };
		float [] pauses = new float[] { 0.5f, 0f, 0f};
		float[] speedMultipliers = new float[] { 0.1f, 1f, 1f };
		ShipManager.GetManager().ShowShipMotion(sortingOrder, positions, pauses, sizes, speedMultipliers, Done);
	}

	public override UIButton GetPointedButton() {
		if (DataOverlayManager.GetManager().Active)
			return DataOverlayManager.GetManager().GetPointedButton();
		return buttons[highestOpen];
	}

	public override UIButton[] GetAllButtons() {
		if (DataOverlayManager.GetManager().Active)
			return DataOverlayManager.GetManager().GetAllButtons();
        UIButton[] uiButtons = new UIButton[buttons.Count + 1];
        for (int i = 0; i < buttons.Count; ++i)
            uiButtons[i] = buttons[i];
        uiButtons[uiButtons.Length - 1] = backButton;
        return uiButtons;
    }
}
