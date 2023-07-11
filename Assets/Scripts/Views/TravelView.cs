using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TravelView : View {

	[SerializeField] View shipHubView = null;
	[SerializeField] View gridGameView = null;
	[SerializeField] UIButton backButton = null;
	[SerializeField] GameObject planetScreen = null;
	[SerializeField] GameObject travelScreen = null;
	[Space]
	[SerializeField] public LevelBatch[] levelBatches = null;
	[SerializeField] SpriteBatch[] spriteBatches = null;
	[SerializeField] Sprite[] planets = null;
	[SerializeField] View[] minigameViews = null;
	[Space]
	[SerializeField] UIButton upperPlanetButton = null;
	[SerializeField] UIButton lowerPlanetButton = null;
	[Space]
	[SerializeField] Image fluffTop = null;
	[SerializeField] Image fluffMid = null;
	[SerializeField] Image fluffBottom = null;
	[Space]
	[SerializeField] Image whiteCurtain = null;
	[SerializeField] Image scroller = null;
	[SerializeField] Image portal = null;
	[SerializeField] Image staticBack = null;
	[SerializeField] float scrollSpeed = 0;
	[SerializeField] float shipSpeed = 0;
	[SerializeField] float wobbleAmount = 0;
	[SerializeField] RectTransform shipStart = null;
	[SerializeField] RectTransform shipEnd = null;
	[SerializeField] RectTransform portalStart = null;

	LevelSettings[] chosenLevelSetting;
	Sprite[] chosenSprites;

	bool travelDone;
	bool wordsReceived;
	Vector3 targetPosition;
	
	int minigameThreshold = 5;
	int minigameIndex;
	int chosenMinigameIndex;

	[System.Serializable]
	public struct LevelBatch {
		public LevelSettings[] settings;

		public LevelBatch(LevelSettings[] settings) {
			this.settings = settings;
		}
	}

	[System.Serializable]
	struct SpriteBatch {
		public Sprite[] sprites;

		public SpriteBatch(Sprite[] sprites) {
			this.sprites = sprites;
		}
	}

	protected override void Initialize() {
		base.Initialize();
		backButton.SubscribePress(Back);
		upperPlanetButton.SubscribePress(() => { PlanetPressed(0); });
		lowerPlanetButton.SubscribePress(() => { PlanetPressed(1); });
		
		// We do not use lower planet for now		
		lowerPlanetButton.gameObject.SetActive(false);

		GameMaster.Instance.CompleteCount = Random.Range(0, Mathf.CeilToInt(minigameThreshold/2f));
	}

	public override void Activate() {
		base.Activate();
		doExitFluff = true;
		StartCoroutine(TravelRoutine());
	}

	void PrepareButtons(int largestModuleIndex, int level_index, int setting_index) {
		largestModuleIndex = Mathf.Min(levelBatches.Length - 1, largestModuleIndex);

		// Adjust level_index and setting_index to be within the valid range
		// Here, Mathf.Clamp(value, min, max) is used to ensure that level_index and setting_index are within the valid range. 
    	level_index = Mathf.Clamp(level_index, 0, levelBatches.Length - 1);
    	setting_index = Mathf.Clamp(setting_index, 0, levelBatches[level_index].settings.Length - 1);
		
		// TODO we no longer choose level randomly
		chosenLevelSetting = new LevelSettings[] {levelBatches[level_index].settings[setting_index], levelBatches[level_index].settings[setting_index]};
		// Debug.Log("TravelView-chosenLevels: " + chosenLevelSetting[0] + chosenLevelSetting[1]);
		chosenSprites = new Sprite[] { spriteBatches[level_index].sprites.GetRandom(), spriteBatches[level_index].sprites.GetRandom() };
		upperPlanetButton.SetSprite(planets[level_index]);
		lowerPlanetButton.SetSprite(planets[level_index]);
		// List<int> availableIndices = new List<int>();
		
		// List<List<LevelSettings>> suitableLevels = new List<List<LevelSettings>>();
		// int wordCount = WordMaster.Instance.MaxCards;

		// Debug.Log("TravelView-LargestMOduleIndex: " + largestModuleIndex);
    
		// for (int i = 0; i <= largestModuleIndex; ++i) {
		// 	suitableLevels.Add(new List<LevelSettings>());
		// 	bool applicable = false;
		// 	foreach (LevelSettings ls in levelBatches[i].settings) {
		// 		if (ls.minWords <= wordCount && ls.maxWords >= wordCount) {
		// 			if (!applicable) {
		// 				applicable = true;
		// 				availableIndices.Add(i);
		// 			}
		// 			suitableLevels[suitableLevels.Count - 1].Add(ls);
		// 		}
		// 	}
		// }

		// Debug.Log("TravelView-availableIndices.Count: " + availableIndices.Count);
		// Debug.Log("TravelView-suitableLevels.Count: " + suitableLevels.Count);

		// // This will choose (with probability) from two batches
		// // so that the lower the level the lower the probability (by half)
		// // The first batch decide the upper planet
		// // The second batch decide the lower planet		
		// int[] chosenBatches = ChooseBatches(availableIndices);		
		// Debug.Log("TravelView-chosenBatches: " + chosenBatches[0] + chosenBatches[1]);

		// // This wil randomly choose two level settings from suitable levels at [chosen batch]
		// //
		// chosenLevelSetting = new LevelSettings[] { suitableLevels[chosenBatches[0]].GetRandom(), suitableLevels[chosenBatches[1]].GetRandom() };
		// Debug.Log("TravelView-chosenLevels: " + chosenLevelSetting[0] + chosenLevelSetting[1]);
		
		// chosenSprites = new Sprite[] { spriteBatches[chosenBatches[0]].sprites.GetRandom(), spriteBatches[chosenBatches[1]].sprites.GetRandom() };
		// upperPlanetButton.SetSprite(planets[chosenBatches[0]]);
		// lowerPlanetButton.SetSprite(planets[chosenBatches[1]]);


		
		// TODO We do not randomize minigame in the test
		bool isMinigame = shipHubView.GetComponent<ShipHubView>().isMinigame;				
		if (isMinigame) {
			//minigameIndex = Random.Range(0, 2);
			// Always play mini game
			minigameIndex = 0;
			chosenMinigameIndex = Random.Range(0, minigameViews.Length);
			if (minigameIndex == 0) {
				Debug.Log("TravelView-Play mini game");
				upperPlanetButton.SetSprite(minigameViews[chosenMinigameIndex].GetComponent<IMinigame>().GetIcon());
			}
			else
				lowerPlanetButton.SetSprite(minigameViews[chosenMinigameIndex].GetComponent<IMinigame>().GetIcon());
		} else
			minigameIndex = -1;


		// Debug.Log("TravelView-Instance.CompleteCount: " + GameMaster.Instance.CompleteCount);
		// if (GameMaster.Instance.CompleteCount >= minigameThreshold) {
		// 	//minigameIndex = Random.Range(0, 2);
		// 	// Always play mini game
		// 	minigameIndex = 0;
		// 	chosenMinigameIndex = Random.Range(0, minigameViews.Length);
		// 	if (minigameIndex == 0) {
		// 		Debug.Log("TravelView-Play mini game");
		// 		upperPlanetButton.SetSprite(minigameViews[chosenMinigameIndex].GetComponent<IMinigame>().GetIcon());
		// 	}
		// 	else
		// 		lowerPlanetButton.SetSprite(minigameViews[chosenMinigameIndex].GetComponent<IMinigame>().GetIcon());
		// } else
		// 	minigameIndex = -1;
	}

	void PlanetPressed(int index) {
		targetPosition = (index == 0) ? upperPlanetButton.transform.position : lowerPlanetButton.transform.position;
		if (index == minigameIndex) {
			ViewManager.GetManager().ShowView(minigameViews[chosenMinigameIndex]);
		} else {
			GameMaster.Instance.SetLevel(chosenLevelSetting[index]);
			GameMaster.Instance.Background = chosenSprites[index];
			ViewManager.GetManager().ShowView(gridGameView);
		}
	}

	int[] ChooseBatches(List<int> availableIndices) {
		if (availableIndices.Count < 1) {
			Debug.LogError("Less than 2 modules available");
			return null;
		}
		int[] chosenBatches = new int[2];
		for (int i = 0; i < chosenBatches.Length; ++i) {

			float random = Random.Range(0, Mathf.Pow(2, availableIndices.Count - 1 - i)); // The largest group has half the chance, second largest group half of that and so forth
			for (int j = 0; j < availableIndices.Count - i; ++j) {
				random -= Mathf.Max(1, Mathf.Pow(2, j - 1));
				if (random <= 0) {
					chosenBatches[i] = (i > 0 && chosenBatches[i - 1] <= j) ? availableIndices[j + 1] : availableIndices[j];
					break;
				}
			}
		}
		return chosenBatches;
	}

	IEnumerator TravelRoutine() {
		scroller.material.SetTextureOffset("_MainTex", Vector2.zero);
		planetScreen.SetActive(false);
		travelScreen.SetActive(true);
		portal.gameObject.SetActive(false);
		scroller.material.SetTextureOffset("_MainTex", Vector2.zero);
		Cosmetic top = CosmeticManager.GetManager().GetEquippedCosmetic(CosmeticSlot.ShipTop);
		Cosmetic mid = CosmeticManager.GetManager().GetEquippedCosmetic(CosmeticSlot.ShipMid);
		Cosmetic bottom = CosmeticManager.GetManager().GetEquippedCosmetic(CosmeticSlot.ShipBottom);
		NetworkManager.GetManager().GetWordList(WordsReceived);

		if (!DebugMaster.Instance.skipTransitions) {
			ShipManager.GetManager().SetShipSprites((top != null) ? top.sprite : null, (mid != null) ? mid.sprite : null, (bottom != null) ? bottom.sprite : null);
			travelDone = false;
			wordsReceived = false;
			InputManager.GetManager().SendingInputs = false;
			yield return new WaitForSeconds(Const.SHIP_START_DELAY);
			InputManager.GetManager().SendingInputs = false;
			ShipManager.GetManager().ShowShipMovement(sortingOrder, false, new Vector3[] { shipStart.position, shipEnd.position }, new float[] { 0 },
				new Vector3[] { shipStart.rect.size, shipEnd.rect.size }, new float[] { shipSpeed * (Screen.height / 1920f) },
				null, () => { travelDone = true; });
			while (!travelDone) {
				yield return null;
			}

			ShipManager.GetManager().StartWobble(wobbleAmount * 1920f / Screen.height);
			float a = 0, b = 0;
			float portalTime = Const.PORTAL_TIME;
			float minWait = Const.MIN_WAIT;
			float accelDuration = Const.ACCEL_DURATION;
			Color staticColor = staticBack.color;
			while (b < portalTime) {
				if (a > accelDuration + minWait && wordsReceived)
					b += Time.deltaTime;
				a += Time.deltaTime;
				staticBack.color = new Color(staticColor.r, staticColor.g, staticColor.b, Mathf.Max(0, accelDuration - a));
				scroller.material.SetTextureOffset("_MainTex", a * Vector2.up * scrollSpeed * (Screen.height / 1920f));
				if (a > accelDuration + minWait && wordsReceived) {
					if (!portal.gameObject.activeSelf)
						portal.gameObject.SetActive(true);
					portal.rectTransform.position = Vector3.Lerp(portalStart.position, shipEnd.position, b / portalTime);
				}
				yield return null;
			}
			staticBack.color = staticColor;
		} else
			while (!wordsReceived)
				yield return null;

		fluffBottom.gameObject.SetActive(true);
		fluffMid.gameObject.SetActive(true);
		fluffTop.gameObject.SetActive(true);

		fluffBottom.sprite = (top != null) ? top.sprite : null;
		fluffMid.sprite = (mid != null) ? mid.sprite : null;
		fluffTop.sprite = (bottom != null) ? bottom.sprite : null;

		ShipManager.GetManager().EndWobble();
		ShipManager.GetManager().HideShip();
		planetScreen.SetActive(true);
		scroller.material.SetTextureOffset("_MainTex", Vector2.zero);
		portal.rectTransform.position = portalStart.position;
		travelScreen.SetActive(false);
		if (!DebugMaster.Instance.skipTransitions) {
			whiteCurtain.gameObject.SetActive(true);
			float a = 0;
			yield return new WaitForSeconds(0.5f);
			while (a < 1) {
				a += Time.deltaTime / 0.5f;
				whiteCurtain.color = new Color(1, 1, 1, 1 - a);
				yield return null;
			}
			whiteCurtain.gameObject.SetActive(false);
			whiteCurtain.color = Color.white;
		}
		InputManager.GetManager().SendingInputs = true;
	}

	public void WordsReceived() {
		wordsReceived = true;
		PrepareButtons(WordMaster.Instance.LargestModuleIndex, WordMaster.Instance.level_index, WordMaster.Instance.setting_index);
	}

	public override void Back() {
		base.Back();
		doExitFluff = false;
		ViewManager.GetManager().ShowView(shipHubView);
	}

	public override void ExitFluff(Callback Done) {
		fluffBottom.gameObject.SetActive(false);
		fluffMid.gameObject.SetActive(false);
		fluffTop.gameObject.SetActive(false);
		ShipManager.GetManager().ShowShipMovement(sortingOrder, false, new Vector3[] { shipEnd.position, targetPosition }, new float[] { 0.5f },
			new Vector3[] { shipEnd.rect.size, Vector2.zero }, new float[] { shipSpeed * (Screen.height / 1920f) },
			null, () => { base.ExitFluff(Done); });
	}

	public override UIButton GetPointedButton() {
		return upperPlanetButton;
	}

	public override UIButton[] GetAllButtons() {
		UIButton[] buttons = new UIButton[] { upperPlanetButton, lowerPlanetButton, backButton };
		return buttons;
	}
}