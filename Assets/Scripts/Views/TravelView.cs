using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TravelView : View {

	[SerializeField] View shipHubView;
	[SerializeField] View gridGameView;
	[SerializeField] UIButton backButton;
	[SerializeField] GameObject planetScreen;
	[SerializeField] GameObject travelScreen;
	[Space]
	[SerializeField] LevelBatch[] levelBatches;
	[SerializeField] SpriteBatch[] spriteBatches;
	[SerializeField] Sprite[] planets;
	[Space]
	[SerializeField] UIButton upperPlanetButton;
	[SerializeField] UIButton lowerPlanetButton;
	[SerializeField] Image fluffShip;
	[Space]
	[SerializeField] Image whiteCurtain;
	[SerializeField] Image scroller;
	[SerializeField] Image portal;
	[SerializeField] float scrollSpeed;
	[SerializeField] float shipSpeed;
	[SerializeField] float wobbleAmount;
	[SerializeField] RectTransform shipStart;
	[SerializeField] RectTransform shipEnd;
	[SerializeField] RectTransform portalStart;

	LevelSettings[] chosenLevels;
	Sprite[] chosenSprites;

	bool travelDone;
	Vector3 targetPosition;

	[System.Serializable]
	struct LevelBatch {
		public LevelSettings[] settings;
	}

	[System.Serializable]
	struct SpriteBatch {
		public Sprite[] sprites;
	}

	protected override void Initialize() {
		base.Initialize();
		backButton.SubscribePress(Back);
		upperPlanetButton.SubscribePress(() => { PlanetPressed(0); });
		lowerPlanetButton.SubscribePress(() => { PlanetPressed(1); });
	}

	public override void Activate() {
		base.Activate();
		doExitFluff = true;
		if (DebugMaster.Instance.skipTransitions)
			PrepareButtons();
		else {
			StartCoroutine(TravelRoutine());
		}
	}

	void PrepareButtons() {
		int largestModuleIndex = Mathf.Min(levelBatches.Length, FakeServerManager.GetManager().GetLargestModuleIndex());
		WordMaster.Instance.SetSamples(FakeServerManager.GetManager().GetCardTypes(), FakeServerManager.GetManager().GetCardWords(), FakeServerManager.GetManager().GetCardPar());

		List<int> availableIndices = new List<int>();
		List<List<LevelSettings>> suitableLevels = new List<List<LevelSettings>>();
		int wordCount = WordMaster.Instance.MaxCards;
		for (int i = 0; i <= largestModuleIndex; ++i) {
			suitableLevels.Add(new List<LevelSettings>());
			bool applicable = false;
			foreach (LevelSettings ls in levelBatches[i].settings) {
				if (ls.minWords <= wordCount && ls.maxWords >= wordCount) {
					if (!applicable) {
						applicable = true;
						availableIndices.Add(i);
					}
					suitableLevels[suitableLevels.Count - 1].Add(ls);
				}
			}
		}
		int[] chosenBatches = ChooseBatches(availableIndices);

		chosenLevels = new LevelSettings[] { suitableLevels[chosenBatches[0]].GetRandom(), suitableLevels[chosenBatches[1]].GetRandom() };
		chosenSprites = new Sprite[] { spriteBatches[chosenBatches[0]].sprites.GetRandom(), spriteBatches[chosenBatches[1]].sprites.GetRandom() };
		upperPlanetButton.SetSprite(planets[chosenBatches[0]]);
		lowerPlanetButton.SetSprite(planets[chosenBatches[1]]);
	}

	void PlanetPressed(int index) {
		targetPosition = (index == 0) ? upperPlanetButton.transform.position : lowerPlanetButton.transform.position;
		GameMaster.Instance.SetLevel((LevelSettings)chosenLevels[index]);
		GameMaster.Instance.Background = chosenSprites[index];
		ViewManager.GetManager().ShowView(gridGameView);
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
		MovingImageManager.GetManager().SetMoverSprite(CosmeticManager.GetManager().GetEquippedCosmetic(CosmeticSlot.Ship).sprite);
		travelDone = false;
		yield return new WaitForSeconds(1.5f);
		InputManager.GetManager().SendingInputs = false;
		MovingImageManager.GetManager().ShowMovingImage(sortingOrder, false, new Vector3[] { shipStart.position, shipEnd.position }, new float[] { 0 },
			new Vector3[] { shipStart.rect.size, shipEnd.rect.size }, new float[] { shipSpeed * (Screen.height / 1920f)},
			null, TravelDone);
		while (!travelDone) {
			yield return null;
		}
		MovingImageManager.GetManager().StartWobble(wobbleAmount * 1920f / Screen.height);
		float a = 0;
		float portalTime = 0.33f;
		while (a < 3) {
			a += Time.deltaTime;
			scroller.material.SetTextureOffset("_MainTex", a * Vector2.up * scrollSpeed * (Screen.height / 1920f));
			if (a > 3 - portalTime) {
				if (!portal.gameObject.activeSelf)
					portal.gameObject.SetActive(true);
				portal.rectTransform.position = Vector3.Lerp(portalStart.position, shipEnd.position, (a - (3 - portalTime)) / portalTime);
			}
			yield return null;
		}
		fluffShip.gameObject.SetActive(true);
		fluffShip.sprite = CosmeticManager.GetManager().GetEquippedCosmetic(CosmeticSlot.Ship).sprite;
		MovingImageManager.GetManager().EndWobble();
		MovingImageManager.GetManager().HideMover();
		planetScreen.SetActive(true);
		scroller.material.SetTextureOffset("_MainTex", Vector2.zero);
		portal.rectTransform.position = portalStart.position;
		travelScreen.SetActive(false);
		whiteCurtain.gameObject.SetActive(true);
		PrepareButtons();
		yield return new WaitForSeconds(0.5f);
		a = 0;
		while (a < 1) {
			a += Time.deltaTime / 0.5f;
			whiteCurtain.color = new Color(1, 1, 1, 1 - a);
			yield return null;
		}
		whiteCurtain.gameObject.SetActive(false);
		whiteCurtain.color = Color.white;
		InputManager.GetManager().SendingInputs = true;
	}

	public void TravelDone() {
		travelDone = true;
	}

	void Back() {
		doExitFluff = false;
		ViewManager.GetManager().ShowView(shipHubView);
	}

	public override void ExitFluff(Callback Done) {
		fluffShip.gameObject.SetActive(false);
		MovingImageManager.GetManager().ShowMovingImage(sortingOrder, false, new Vector3[] { shipEnd.position, targetPosition }, new float[] { 0.5f },
			new Vector3[] { shipEnd.rect.size, Vector2.zero }, new float[] { shipSpeed * (Screen.height / 1920f) },
			null, ()=> { base.ExitFluff(Done); });
	}

	public override UIButton GetPointedButton() {
		return upperPlanetButton;
	}

	public override UIButton[] GetAllButtons() {
		UIButton[] buttons = new UIButton[] { upperPlanetButton, lowerPlanetButton, backButton };
		return buttons;
	}
}
