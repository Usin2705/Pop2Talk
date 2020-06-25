using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TravelView : View {

	[SerializeField] View shipHubView;
	[SerializeField] View gridGameView;
	[SerializeField] UIButton backButton;
	[Space]
	[SerializeField] LevelBatch[] levelBatches;
	[SerializeField] SpriteBatch[] spriteBatches;
	[SerializeField] Sprite[] planets;
	[Space]
	[SerializeField] UIButton upperPlanetButton;
	[SerializeField] UIButton lowerPlanetButton;
	[Space]
	[SerializeField] Image scroller;
	[SerializeField] float scrollSpeed;

	LevelSettings[] chosenLevels;
	Sprite[] chosenSprites;

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
		GameMaster.Instance.SetLevel((GridLevelSettings)chosenLevels[index]);
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

	void Update() {
		scroller.material.SetTextureOffset("_MainTex", Time.time * Vector2.up * scrollSpeed * (Screen.height/1920f));
	}

	void Back() {
		ViewManager.GetManager().ShowView(shipHubView);
	}

	public override UIButton GetPointedButton() {
		return upperPlanetButton;
	}

	public override UIButton[] GetAllButtons() {
		UIButton[] buttons = new UIButton[] { upperPlanetButton, lowerPlanetButton, backButton };
		return buttons;
	}
}
