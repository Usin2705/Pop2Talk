using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameView : View, IGameCaller {

	[SerializeField] View stageHub;
	[Space]
	[SerializeField] SpeechCollection noClearSpeech;
	[SerializeField] SpeechCollection clearNoMedalSpeech;
	[SerializeField] SpeechCollection clearGetMedalSpeech;
	[Space]
	[SerializeField] Image background;
	[SerializeField] GameUIHandler gameUI;
	[SerializeField] UIButton backButton;
    [Space]
    [SerializeField] [Range(0.0f, 1.0f)]float cheerChance = 0.25f;

	StageSettings stage;
	WordData currentWord;

	float levelDuration = 0f;

	int TotalStars { get; set; }

	bool musicFaded;

    bool firstCheerDone = false;

	void Update()
	{
		levelDuration += Time.deltaTime;
	}

	protected override void Initialize() {
		base.Initialize();
		backButton.SubscribePress(Back);
	}
	
	public override void Activate() {
		base.Activate();
		stage = PlanetManager.GetManager().GetStage();
		background.sprite = stage.background;
		int firsts = 0;
		foreach (WordListSettings ws in stage.wordLists) {
			if (ws.first)
				firsts += ws.wordList.GetWords().Length;
		}
		int adaptiveWords = Mathf.Max(Mathf.Min(stage.totalCards - firsts-stage.forcedWords.Length, stage.adaptiveWords));
		if (adaptiveWords != stage.adaptiveWords)
			Debug.Log("Too many adaptive words for amount of new words");
		WordMaster.Instance.SetWords(stage.wordLists, stage.totalCards, stage.quizCards, adaptiveWords, stage.cardPerRound, stage.forcedWords);
		GameMaster.Instance.Initialize(stage.level, this);
		gameUI.SetTrackedIcon((stage.level.trackClicks) ? IconManager.GetManager().clickIcon : IconManager.GetManager().moonstoneIcon);
		gameUI.SetCardBar(true);
		BeginGame();
	}

	void BeginGame() {
		gameUI.SetTrackedValue(0, true);
		gameUI.SetStars(0, true);
		gameUI.SetCardsLeft(stage.totalCards, true);
		levelDuration = 0f;
		GameMaster.Instance.LaunchGame(stage.totalCards == 1);
	}

	public void Clicked() {
		if (stage.popSoundType == PopSoundType.Next && (stage.popSoundIsWordForQuiz || WordMaster.Instance.PeekNextType() != WordCardType.Quiz)) {
			ToggleMusic(0.25f, false);
			musicFaded = true;
			WordData word = WordMaster.Instance.GetPopWord(GameMaster.Instance.RemainingProgress / GameMaster.Instance.MaxProgress);
			NetworkManager.GetManager().SamplePlayed(stage.name, word.name, false);
			GameMaster.Instance.SetCustomClip(word.languageWords[LanguageManager.GetManager().TargetLanguage].pronunciations.GetRandom());
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
		bool done = TotalStars >= stage.starsRequired;
		if (done)
			StageSettings.SetDoneStatus(stage, NetworkManager.GetManager().Player);
		bool medal = GameMaster.Instance.GetMedal();
		if (TotalStars > StageSettings.GetStars(stage, NetworkManager.GetManager().Player)) {
			// TODO: ZZZ-lowprio More specific cheers for compeleting level?
			StageSettings.SetStars(stage, TotalStars, NetworkManager.GetManager().Player);
		}
		if (medal)
			LevelSettings.SetMedal(stage.level, NetworkManager.GetManager().Player);
		if (GameMaster.Instance.TrackedValue > LevelSettings.GetHiscore(stage.level, NetworkManager.GetManager().Player))
			LevelSettings.SetHiscore(stage.level, GameMaster.Instance.TrackedValue, NetworkManager.GetManager().Player);
		NetworkManager.GetManager().LevelCompleteEvent("level_complete", stage.name, stage.level.gameType.ToString(), levelDuration, (TotalStars / stage.totalCards) , TotalStars, GameMaster.Instance.TrackedValue, medal);
		SpeechCollection speech = !firstCheerDone ? (done ? (medal ? clearGetMedalSpeech : clearNoMedalSpeech) : noClearSpeech) : Random.Range(0,1) >= cheerChance ? (done ? (medal ? clearGetMedalSpeech : clearNoMedalSpeech) : noClearSpeech) : noClearSpeech;
        if (speech != noClearSpeech && !firstCheerDone)
            firstCheerDone = true;
        Debug.Log(firstCheerDone);
		Sprite[] icons = new Sprite[2];
		if (IconManager.GetManager() != null) {
			icons[0] = IconManager.GetManager().starIcon;
			icons[1] = (stage.level.trackClicks) ? IconManager.GetManager().clickIcon : IconManager.GetManager().moonstoneIcon;
		}
		Callback[] actions = new Callback[2] { ()=> { GameMaster.Instance.LaunchGame(stage.totalCards == 1); }, Back };
		Sprite[] buttons = new Sprite[2];
		buttons[0] = IconManager.GetManager().replayIcon;
		buttons[1] = IconManager.GetManager().arrowIcon;
		DataOverlayManager.GetManager().Show(sortingOrder, false, "", icons, PlanetManager.GetManager().GetStageStats(false, TotalStars, GameMaster.Instance.TrackedValue), actions, buttons, () => { GameOverDelay(speech); });
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
		WordMaster.Instance.Initialize();
		WordMaster.Instance.ChooseNextWordBatch();
		TotalStars = 0;
	}

	public void RoundDone() {
		if (DebugSettings.Instance.skipWords) {
			int stars = 0;
			while (WordMaster.Instance.RemainingBatch > 0) {
				WordMaster.Instance.UseNextWord();
				stars += 5;
			}
			CardDone(stars);
		} else {
			ToggleMusic(0.25f, false);

			ShowCard();
		}
	}

	void ShowCard() {
		WordCardType nextType = WordMaster.Instance.UseNextType();
		currentWord = WordMaster.Instance.UseNextWord();
		if (currentWord == null)
			CardDone(0);
		else
			WordMaster.Instance.ShowWordCard(nextType, stage.name, currentWord, LanguageManager.GetManager().TargetLanguage, LanguageManager.GetManager().NativeLanguage, sortingOrder, CardDone);
	}

	void CardDone(int stars) {
		TotalStars += stars;
		gameUI.SetStars(TotalStars);
		gameUI.SetCardsLeft(WordMaster.Instance.CardsRemaining);
		if (currentWord != null)
			WordMaster.Instance.RecordStarAmount(currentWord.name, stars);
		if (WordMaster.Instance.RemainingBatch > 0)
			ShowCard();
		else if (WordMaster.Instance.CardsRemaining > 0) {
			ToggleMusic(1f, true);
			WordMaster.Instance.ChooseNextWordBatch();
			if (WordMaster.Instance.CardsRemaining == 1)
				GameMaster.Instance.FinalRound = true;
			GameMaster.Instance.StartRound();
		} else {
			GameDone();
			ToggleMusic(1f, true);
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

	public int GetMaxRounds() {
		return Mathf.CeilToInt(stage.totalCards/(float)stage.cardPerRound);
	} 

	public void Back() {
		NetworkManager.GetManager().LevelAbortEvent("level_abort", stage.name, stage.level.gameType.ToString(), levelDuration, stage.totalCards - WordMaster.Instance.CardsRemaining, (TotalStars / stage.totalCards), TotalStars, GameMaster.Instance.TrackedValue);
		doExitFluff = false;
		ViewManager.GetManager().ShowView(stageHub);
	}

	public void SetSpecificUI(List<MatchType> matchTypes) {
		gameUI.SetSpecificUI(matchTypes);
	}

	public override UIButton GetPointedButton() {
		return null;
	}

	public override UIButton[] GetAllButtons() {
        return null;
	}
}
