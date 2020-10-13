﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordPearlView : View {

	[SerializeField] View shipHubView = null;
	[Space]
	[SerializeField] UIButton backButton = null;
	[SerializeField] GameObject wordPearlPrefab = null;
	[SerializeField] GridPageHandler gridHandler = null;

	Dictionary<string, PearlStars> pearls = new Dictionary<string, PearlStars>();
	string currentWord;
	
	protected override void Initialize() {
		base.Initialize();
		backButton.SubscribePress(Back);
	}

	public override void Activate() {
		base.Activate();
		Dictionary<string, int> bestResults = WordMaster.Instance.GetBestResults();
		foreach (string s in bestResults.Keys) {
			if (!pearls.ContainsKey(s)) {
				pearls.Add(s, (Instantiate(wordPearlPrefab, gridHandler.GetNextParent(0)).GetComponent<PearlStars>()));
				pearls[s].SetUp(s, this);
			}
			pearls[s].SetStars(bestResults[s]);
		}
	}

	public override void Deactivate() {
		base.Deactivate();
		WordCardManager.GetManager().StopCard();
	}

	public void ShowCard(string word) {
		currentWord = word;
		SoundEffectManager.GetManager().FadeMusic(0.25f, 0);
		WordMaster.Instance.ShowWordCard(WordCardType.Repeat, "Word Pearl Rehearse", WordMaster.Instance.StringToWordData(currentWord), sortingOrder, Done);
	}

	public override void Back() {
		base.Back();
		ViewManager.GetManager().ShowView(shipHubView);
	}

	void Done(int stars) {
		SoundEffectManager.GetManager().FadeMusic(0.25f, 1);
		if (stars > pearls[currentWord].Stars)
			pearls[currentWord].SetAnimatedStars(stars, 1f);
	}

	public override UIButton GetPointedButton() {
		return null;
	}

	public override UIButton[] GetAllButtons() {
		return null;
	}
}
