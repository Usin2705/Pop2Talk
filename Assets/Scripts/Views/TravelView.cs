using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
		upperPlanetButton.SubscribePress(()=> { PlanetPressed(0); });
		lowerPlanetButton.SubscribePress(() => { PlanetPressed(1); });
	}

	public override void Activate() {
		base.Activate();
		int largestModuleIndex = Mathf.Min(levelBatches.Length, FakeServerManager.GetManager().GetLargestModuleIndex());
		WordMaster.Instance.SetSamples(FakeServerManager.GetManager().GetCardTypes(), FakeServerManager.GetManager().GetCardWords(), FakeServerManager.GetManager().GetCardPar());
		int[] chosenBatches = ChooseBatches(largestModuleIndex);
		chosenLevels = new LevelSettings[] { levelBatches[chosenBatches[0]].settings.GetRandom(), levelBatches[chosenBatches[1]].settings.GetRandom() };
		chosenSprites = new Sprite[] { spriteBatches[chosenBatches[0]].sprites.GetRandom(), spriteBatches[chosenBatches[1]].sprites.GetRandom() };
		Sprite[] chosenPlanet = new Sprite[] { planets[chosenBatches[0]], planets[chosenBatches[1]] };
		upperPlanetButton.SetSprite(chosenPlanet[0]);
		lowerPlanetButton.SetSprite(chosenPlanet[1]);
	}

	void PlanetPressed(int index) {
		GridGameMaster.Instance.SetLevel((GridLevelSettings)chosenLevels[index]);
		GridGameMaster.Instance.Background = chosenSprites[index];
		ViewManager.GetManager().ShowView(gridGameView);
	}

	int[] ChooseBatches(int largestModuleIndex) {//first largest group has index of 0;
		if (largestModuleIndex < 1) {
			Debug.LogError("Less than 2 modules available");
			return null;
		}
		int[] chosenBatches = new int[2];
		for (int i = 0; i < chosenBatches.Length; ++i) {

			float random = Random.Range(0, Mathf.Pow(2, largestModuleIndex - i)); // The largest group has half the chance, second largest group half of that and so forth
			for (int j = 0; j < largestModuleIndex + 1 - i; ++j) {
				random -= Mathf.Max(1, Mathf.Pow(2, j - 1));
				if (random <= 0) {
					chosenBatches[i] = (i > 0 && chosenBatches[i - 1] <= j) ? j + 1 : j;
					break;
				}
			}
		}
		return chosenBatches;
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
