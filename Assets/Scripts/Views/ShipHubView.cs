using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

	[SerializeField] TMP_Dropdown levelDD = null;

	[SerializeField] TMP_Dropdown settingsDD = null;

	[SerializeField] Toggle miniToggle = null;

	[SerializeField] public int levelIndex = 0;
	[SerializeField] public int settingsIndex = 0;
	[SerializeField] public bool isMinigame = false;
	List<string> lvSettingsName = new List<string>() {"Classic 1 - 6x6", "Classic 2 - 6x8", "Clear 1 - 6x6 - 4 types", "Clear 2 - 6x8 - 4 types"};

	protected override void Initialize() {
		base.Initialize();


		travelButton.SubscribePress(GotoTravelView);
		storeButton.SubscribePress(GotoStoreView);
		pearlButton.SubscribePress(GotoPearlView);

		//Add listener for when the value of the Dropdown changes, to take action
		levelDD.onValueChanged.AddListener(delegate {
            LevelDDValueChanged(levelDD);
        });

		settingsDD.onValueChanged.AddListener(delegate {
            LevelSettingsDDValueChanged(settingsDD);
        });
		settingsDD.ClearOptions();
		settingsDD.AddOptions(lvSettingsName);

		miniToggle.isOn = false;
		miniToggle.onValueChanged.AddListener(delegate {
            MinigameToggleChanged(miniToggle);
		});

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

	void LevelDDValueChanged(TMP_Dropdown dd)
    {
		levelIndex = dd.value;		
        		
		//TravelView.LevelBatch[] test = travelView.GetComponent<TravelView>().levelBatches;
		//Debug.Log(test[0].settings[0].ToString());
		// This will setup the second dropdown options
		settingsIndex = 0;
		settingsDD.ClearOptions();
		lvSettingsName.Clear();
		foreach (LevelSettings levelSettings in travelView.GetComponent<TravelView>().levelBatches[levelIndex].settings) 
		//settingsDD
		{
			string dropName = levelSettings.ToString();
			lvSettingsName.Add(dropName.Remove(dropName.Length - 16));	
			//Debug.Log(dropName.Remove(dropName.Length - 16));
		}		

		settingsDD.AddOptions(lvSettingsName);
    }

	void LevelSettingsDDValueChanged(TMP_Dropdown dd)
    {
		settingsIndex = dd.value;		
    }	

	void MinigameToggleChanged(Toggle toggle)
    {
		isMinigame = toggle.isOn;		
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
