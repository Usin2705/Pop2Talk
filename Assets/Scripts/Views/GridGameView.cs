using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridGameView : View, IGameCaller {

	[SerializeField] View shipHub;
	[SerializeField] View finishView;
	[Space]
	[SerializeField] SpeechCollection noClearSpeech;
	[SerializeField] SpeechCollection clearNoMedalSpeech;
	[SerializeField] SpeechCollection clearGetMedalSpeech;
	[Space]
	[SerializeField] Image background;
	[SerializeField] GameUIHandler gameUI;
	[SerializeField] UIButton backButton;
	[Space]
	[SerializeField] [Range(0.0f, 1.0f)] float cheerChance = 0.25f;

	float levelDuration = 0f;


	bool musicFaded;

	bool firstCheerDone = false;

	void Update() {
		levelDuration += Time.deltaTime;
	}

	protected override void Initialize() {
		base.Initialize();
		backButton.SubscribePress(Back);
	}

	public override void Activate() {
		base.Activate();
		background.sprite = GridGameMaster.Instance.Background;
		GridGameMaster.Instance.SetGameCaller(this);
		gameUI.SetCardBar(true);
		BeginGame();
	}

	void BeginGame() {
		WordMaster.Instance.MakeQueue();
		gameUI.SetTrackedValue(0, true);
		gameUI.SetStars(0, true);
		gameUI.SetCardsLeft(WordMaster.Instance.CardsRemaining, true);
		levelDuration = 0f;
		GridGameMaster.Instance.LaunchGame(WordMaster.Instance.CardsRemaining == 1);
	}

	public void Clicked() {
		if (WordMaster.Instance.PeekNextType() != WordCardType.Memory) {
			ToggleMusic(0.25f, false);
			musicFaded = true;
			WordData word = WordMaster.Instance.PeekNextWord();
			NetworkManager.GetManager().SamplePlayed(GridGameMaster.Instance.CurrentLevel.name, word.name, true);
			GridGameMaster.Instance.SetCustomClip(word.pronunciations.GetRandom());
		} else
			GridGameMaster.Instance.SetCustomClip(null);
	}

	public void ClickDone() {
		ToggleMusic(0.5f, true);
	}

	void ToggleMusic(float duration, bool on) {
		if (musicFaded != on)
			return;
		SoundEffectManager.GetManager().FadeMusic(duration, (on) ? 1f : 0f);
		musicFaded = !on;
	}

	void GameDone() {
		ViewManager.GetManager().ShowView(finishView);
	}

	void GameOverDelay(SpeechCollection speech) {
		if (!DebugSettings.Instance.skipTransitions)
			StartCoroutine(GameOverDelayRoutine(speech, 1f));
		else
			InputManager.GetManager().SendingInputs = true;
	}

	IEnumerator GameOverDelayRoutine(SpeechCollection speech, float delay) {
		yield return new WaitForSeconds(delay);
		CharacterManager.GetManager().ShowCharacter(speech, sortingOrder + 1, null); // +1 to go over dataoverlay
	}

	public void LaunchSetup() {
		WordMaster.Instance.MakeQueue();
		WordMaster.Instance.TotalStars = 0;
	}

	public void RoundDone() {
		if (DebugSettings.Instance.skipWords) {
			WordMaster.Instance.Dequeue();
			CardDone(5);
		} else {
			ToggleMusic(0.25f, false);

			ShowCard();
		}
	}

	void ShowCard() {
		WordCardType nextType = WordMaster.Instance.PeekNextType();
		WordData currentWord = WordMaster.Instance.PeekNextWord();
		WordMaster.Instance.Dequeue();
		if (currentWord == null)
			CardDone(0);
		else
			WordMaster.Instance.ShowWordCard(nextType, GridGameMaster.Instance.CurrentLevel.name, currentWord, sortingOrder, CardDone);
	}

	void CardDone(int stars) {
		WordMaster.Instance.TotalStars += stars;
		gameUI.SetStars(WordMaster.Instance.TotalStars);
		gameUI.SetCardsLeft(WordMaster.Instance.CardsRemaining);
		if (WordMaster.Instance.CardsRemaining > 0) {
			ToggleMusic(1f, true);
			if (WordMaster.Instance.CardsRemaining == 1)
				GridGameMaster.Instance.FinalRound = true;
			GridGameMaster.Instance.StartRound();
		} else {
			GameDone();
			ToggleMusic(1f, true);
		}
	}

	public override void Deactivate() {
		base.Deactivate();
		gameUI.SetCardBar(false);
		WordCardManager.GetManager().StopCard();
		GridGameMaster.Instance.Back();
	}

	public void SetProgress(float progress) {
		gameUI.SetProgress(progress);
	}

	public void SetTrackedValue(int value) {
		gameUI.SetTrackedValue(value, value == 0);
	}

	public void Back() {
		NetworkManager.GetManager().LevelAbortEvent("level_abort", GridGameMaster.Instance.CurrentLevel.name, GridGameMaster.Instance.CurrentLevel.gameMode.ToString(), levelDuration, 
			WordMaster.Instance.MaxCards - WordMaster.Instance.CardsRemaining, WordMaster.Instance.TotalStars / WordMaster.Instance.MaxCards, WordMaster.Instance.TotalStars, GridGameMaster.Instance.SpaceDust);
		doExitFluff = false;
		ViewManager.GetManager().ShowView(shipHub);
	}

	public override UIButton GetPointedButton() {
		return null;
	}

	public override UIButton[] GetAllButtons() {
		return null;
	}
}
