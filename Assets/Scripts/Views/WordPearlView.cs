using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordPearlView : View {

	[SerializeField] View planetHubView;
	[Space]
	[SerializeField] UIButton backButton;
	[SerializeField] GameObject wordPearlPrefab;
	[SerializeField] Transform gridHolder;

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
				pearls.Add(s, (Instantiate(wordPearlPrefab, gridHolder).GetComponent<PearlStars>()));
				pearls[s].SetUp(s, this);
			}
			pearls[s].SetStars(bestResults[s]);
		}
	}

	public void ShowCard(string word) {
		currentWord = word;
		WordMaster.Instance.ShowWordCard(WordCardType.Rehearse, "Word Pearl Rehearse", WordMaster.Instance.StringToWordData(currentWord), 
			LanguageManager.GetManager().TargetLanguage, LanguageManager.GetManager().NativeLanguage, sortingOrder, Done);
	}

	void Back() {
		ViewManager.GetManager().ShowView(planetHubView);
	}

	void Done(int stars) {
		pearls[currentWord].SetStars(stars);
	}

	public override UIButton GetPointedButton() {
		return null;
	}

	public override UIButton[] GetAllButtons() {
		return null;
	}
}
