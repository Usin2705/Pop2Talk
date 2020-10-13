using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordRushView : View {

	[SerializeField] UIButton backButton = null;
	[SerializeField] UIButton cardButton = null;

	List<string> rushWords = new List<string>();
	int rushIndex = 0;
	string currentWord;


	protected override void Initialize() {
		base.Initialize();
		backButton.SubscribePress(Exit);
		cardButton.SubscribePress(ShowCard);
		cardButton.SetIcon(IconManager.GetManager().arrowIcon);
		rushWords.Clear();
	}

	public override void Activate() {
		base.Activate();
		ShuffleRush();
	}

	void ShuffleRush() {
		rushIndex = 0;
		rushWords.Shuffle();
	}

	void ShowCard() {
		currentWord = rushWords[rushIndex];
		WordMaster.Instance.ShowWordCard(WordCardType.Repeat, "Word Rush", WordMaster.Instance.StringToWordData(currentWord), sortingOrder, Done);
	}

	void Done(int stars) {
		WordMaster.Instance.RecordStarAmount(currentWord, stars, (int) WordCardType.Repeat);
		rushIndex++;
		if (rushIndex == rushWords.Count)
			ShuffleRush();
	}

	void Exit() {
		NetworkManager.GetManager().ControlledExit();
		Application.Quit();
	}

	public override UIButton GetPointedButton() {
		return cardButton;
	}

	public override UIButton[] GetAllButtons() {
		return new UIButton[] { cardButton, backButton };
	}
}
