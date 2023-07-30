using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameView : View, IGameCaller {

	[SerializeField] View shipHub = null;
	[SerializeField] View finishView = null;
	[Space]
	[SerializeField] Image background = null;
	[SerializeField] Image fluffForeground = null;
	[SerializeField] GameUIHandler gameUI = null;
	[SerializeField] UIButton backButton = null;

	float levelDuration = 0f;

	bool musicFaded;
	bool fluffDone = false;

	void Update() {
		if (fluffDone)
			levelDuration += Time.deltaTime;
	}

	protected override void Initialize() {
		base.Initialize();
		backButton.SubscribePress(Back);
	}

	public override void Activate() {
		base.Activate();
		fluffDone = false;
		background.sprite = GameMaster.Instance.Background;
		if (!DebugMaster.Instance.skipTransitions) {
			fluffForeground.gameObject.SetActive(true);
			fluffForeground.sprite = background.sprite;
			fluffForeground.color = Color.white;
		}
		GameMaster.Instance.SetGameCaller(this);
		gameUI.SetCardBar(true);
		BeginGame();
	}

	void BeginGame() {
		WordMaster.Instance.MakeQueue();
		gameUI.SetTrackedValue(0, true);
		gameUI.SetStars(0, true);
		gameUI.SetCardsLeft(WordMaster.Instance.CardsRemaining, true);
		levelDuration = 0f;
		GameMaster.Instance.InitializeGame(WordMaster.Instance.CardsRemaining == 1);
		StartCoroutine(StartDelay());
	}

	IEnumerator StartDelay() {
		while (!fluffDone && !DebugMaster.Instance.skipTransitions)
			yield return null;
		GameMaster.Instance.StartRound();
	}

	public void Clicked() {
		WordData word = WordMaster.Instance.PeekNextWord();
		if (WordMaster.Instance.PeekNextType() != WordCardType.Memory && word != null) {
			ToggleMusic(0.25f, false);
			musicFaded = true;
			// NetworkManager.GetManager().SamplePlayed(GameMaster.Instance.CurrentLevel.name, word.name, true);
			GameMaster.Instance.SetCustomClip(word.pronunciations.GetRandom());
		} else
			GameMaster.Instance.SetCustomClip(null);
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
		if (!DebugMaster.Instance.skipTransitions)
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
		if (DebugMaster.Instance.skipWords) {
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
		{
			Debug.Log("Card get 0");
			CardDone(0);
		}
		else
			WordMaster.Instance.ShowWordCard(nextType, GameMaster.Instance.CurrentLevel.name, currentWord, sortingOrder, CardDone);
	}

	void CardDone(int stars) {
		WordMaster.Instance.TotalStars += stars;
		gameUI.SetStars(WordMaster.Instance.TotalStars);
		gameUI.SetCardsLeft(WordMaster.Instance.CardsRemaining);
		ToggleMusic(1f, true);
		if (WordMaster.Instance.CardsRemaining > 0) {
			if (WordMaster.Instance.CardsRemaining == 1)
				GameMaster.Instance.FinalRound = true;
			GameMaster.Instance.StartRound();
		} else {
			GameDone();
		}
	}

	public override void Deactivate() {
		base.Deactivate();
		gameUI.SetCardBar(false);
		WordCardManager.GetManager().StopCard();
		GameMaster.Instance.Back();
	}

	public void SetProgress(float progress) {
		gameUI.SetProgress(progress);
	}

	public void SetTrackedValue(int value) {
		gameUI.SetTrackedValue(value, value == 0);
	}

	public override void Back() {
		base.Back();
		Debug.Log("GameView Back Button pressed");
		NetworkManager.GetManager().LevelAbortEvent("level_abort", GameMaster.Instance.CurrentLevel.name, GameMaster.Instance.CurrentLevel.gameMode.ToString(), levelDuration,
			WordMaster.Instance.MaxCards - WordMaster.Instance.CardsRemaining, WordMaster.Instance.TotalStars / WordMaster.Instance.MaxCards, WordMaster.Instance.TotalStars, GameMaster.Instance.SpaceDust);
		doExitFluff = false;
		ViewManager.GetManager().ShowView(shipHub);
	}

	public override UIButton GetPointedButton() {
		return null;
	}

	public override UIButton[] GetAllButtons() {
		return null;
	}

	public override void EnterFluff(Callback Done) {
		StartCoroutine(EnterFluffRoutine(Done));
	}

	IEnumerator EnterFluffRoutine(Callback Done) {
		float a = 0, introDuration = 1f, fadeDuration = 0.5f;
		while (a < 1) {
			a += Time.deltaTime / introDuration;
			if (a > 1)
				a = 1;
			yield return null;
		}
		while (a > 0) {
			a -= Time.deltaTime / fadeDuration;
			if (a < 0)
				a = 0;
			fluffForeground.color = new Color(1, 1, 1, a);
			yield return null;
		}
		fluffForeground.gameObject.SetActive(false);
		fluffDone = true;
		Done();
	}
}
