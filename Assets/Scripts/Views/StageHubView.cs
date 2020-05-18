using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageHubView : View {

	[SerializeField] View game;
	[SerializeField] View planetHub;
	[Space]
	[SerializeField] UIButton backButton;
	[SerializeField] Image background;
	[SerializeField] Sprite nodeSprite;
	[SerializeField] Sprite lockedNodeSprite;
	[SerializeField] GameObject nodePrefab;
	[SerializeField] GameObject pathPrefab;
	[SerializeField] Transform layoutRoot;
	[Space]
	[SerializeField] Transform shipHolder;
	Transform buttonRoot;
    [Space]
    [SerializeField] float buttonWobbleAmount = 5;
    [SerializeField] float buttonWobbleSpeed = 2;
    [SerializeField] [Range(1,5)] float highestWobbleMultiplier = 1.25f;

    Dictionary<int, GameObject> instantiatedNodeLayouts = new Dictionary<int, GameObject>();
	Dictionary<int, GameObject> instantiatedPathLayouts = new Dictionary<int, GameObject>();

	PathGUI pathGUI;

	GameObject nodeLayoutPrefab;
	int chosenIndex;
	int currentPlanetIndex;

	int highestOpen;
	List<UIButton> currentButtons = new List<UIButton>();

	private void Awake() {
		pathGUI = GetComponent<PathGUI>();
	}

	private void Start() {
		backButton.SubscribePress(Back);
	}

	public override void Activate() {
		base.Activate();

		int stages = PlanetManager.GetManager().GetStageCount();
		currentPlanetIndex = PlanetManager.GetManager().GetPlanetIndex();
		UIButton button;
		background.sprite = PlanetManager.GetManager().GetCurrentPlanetBackground();
		nodeLayoutPrefab = PlanetManager.GetManager().GetNodeLayoutPrefab();
		if (!instantiatedNodeLayouts.ContainsKey(currentPlanetIndex)) {
			instantiatedNodeLayouts.Add(currentPlanetIndex, Instantiate(nodeLayoutPrefab, layoutRoot));
			buttonRoot = instantiatedNodeLayouts[currentPlanetIndex].transform;

			instantiatedPathLayouts.Add(currentPlanetIndex, Instantiate(nodeLayoutPrefab, layoutRoot));
			
			for (int i = 0; i < stages; ++i) {
				button = Instantiate(nodePrefab, buttonRoot.GetChild(i)).GetComponent<UIButton>();
				int j = i; // If subscribed simply to i, the lambda apparentaly keeps a reference to it and makes all subscriptions equal to stages
				button.SubscribePress(() => { NodePressed(j); });
				button.SetSprite(nodeSprite, lockedNodeSprite);
			}

			pathGUI.SetUp(instantiatedPathLayouts[currentPlanetIndex].transform, pathPrefab, stages);
		} else {
			buttonRoot = instantiatedNodeLayouts[currentPlanetIndex].transform;
			instantiatedNodeLayouts[currentPlanetIndex].SetActive(true);
			instantiatedPathLayouts[currentPlanetIndex].SetActive(true);
		}
		int firstUnclear = PlanetManager.GetManager().GetCurrentFirstUnclear();
		bool locked;
		currentButtons.Clear();
		for (int i = 0; i < stages; ++i) {
			button = instantiatedNodeLayouts[currentPlanetIndex].transform.GetChild(i).GetComponentInChildren<UIButton>();
			locked = i > firstUnclear;
			button.SetLocked(locked);
			if (locked) {
				button.SetIcon(IconManager.GetManager().lockIcon);
                button.GetComponent<ButtonEffects>().SetWobble(0, 0);
            } else {
				highestOpen = i;
                button.GetComponent<ButtonEffects>().SetWobble(buttonWobbleAmount, buttonWobbleSpeed);
				if (PlanetManager.GetManager().GetStageMedal(i)) {
					button.SetIcon(IconManager.GetManager().medalIcon);
				} else {
					button.SetText("" + (currentPlanetIndex + 1) + " - " + (i + 1));
				}
			}
			pathGUI.SetLocked(i, i > firstUnclear, i >= firstUnclear);
			currentButtons.Add(button);
		}
        instantiatedNodeLayouts[currentPlanetIndex].transform.GetChild(highestOpen).GetComponentInChildren<UIButton>().GetComponent<ButtonEffects>().SetWobble(buttonWobbleAmount * highestWobbleMultiplier, buttonWobbleSpeed * highestWobbleMultiplier); ;
    }

	public override void Deactivate() {
		base.Deactivate();
		instantiatedNodeLayouts[currentPlanetIndex].SetActive(false);
		instantiatedPathLayouts[currentPlanetIndex].SetActive(false);
	}

	void NodePressed(int index) {
		chosenIndex = index;
		PlanetManager.GetManager().SetStage(index);
		string[] stats = PlanetManager.GetManager().GetStageStats();
		Sprite[] icons = new Sprite[2];
		if (IconManager.GetManager() != null) {
			icons[0] = IconManager.GetManager().starIcon;
			icons[1] = (PlanetManager.GetManager().GetStage().level.trackClicks) ? IconManager.GetManager().clickIcon : IconManager.GetManager().moonstoneIcon;
		}
		Sprite[] buttonIcons = new Sprite[1];
		if (IconManager.GetManager() != null) {
			buttonIcons[0] = IconManager.GetManager().arrowIcon;
		}
		DataOverlayManager.GetManager().Show(sortingOrder, true, (PlanetManager.GetManager().GetPlanetIndex() + 1) + " - " + (index + 1), icons, stats, new Callback[1] { GotoGameView }, buttonIcons, null);
	}

	void GotoGameView() {
		doExitFluff = true;
		ViewManager.GetManager().ShowView(game);
	}

	void Back() {
		doExitFluff = false;
		ViewManager.GetManager().ShowView(planetHub);
	}

	public override void ExitFluff(Callback Done) {
		Vector3[] positions = new Vector3[] {new Vector3(instantiatedNodeLayouts[currentPlanetIndex].transform.GetChild(chosenIndex).position.x, shipHolder.position.y), instantiatedNodeLayouts[currentPlanetIndex].transform.GetChild(chosenIndex).position };
		Vector3[] sizes = new Vector3[] { Vector3.one * 0.4f, Vector3.one * 0.4f};
		float[] pauses = new float[] { 0f, 0f };
		float[] speedMultipliers = new float[] { 0.6f, 0.6f};
		ShipManager.GetManager().ShowShipMotion(sortingOrder, positions, pauses, sizes, speedMultipliers, Done);
	}
	
	public override UIButton GetPointedButton() {
		if (DataOverlayManager.GetManager().Active)
			return DataOverlayManager.GetManager().GetPointedButton();
		return currentButtons[highestOpen];
	}

	public override UIButton[] GetAllButtons() {
		if (DataOverlayManager.GetManager().Active)
			return DataOverlayManager.GetManager().GetAllButtons();
        UIButton[] uiButtons = new UIButton[currentButtons.Count + 1];
        for (int i = 0; i < currentButtons.Count; ++i)
            uiButtons[i] = currentButtons[i];
        uiButtons[uiButtons.Length - 1] = backButton;
        return uiButtons;
    }
}
