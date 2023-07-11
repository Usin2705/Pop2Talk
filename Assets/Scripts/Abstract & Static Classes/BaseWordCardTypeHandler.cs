using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseWordCardHandler {

    protected static float phaseGap = 0.1f;

	protected IntCallback Done;

	protected int stars = -1;

	WordData currentWord;

	public virtual void ShowCard(WordData wordData, string levelName, int order, IntCallback Done) {
		this.Done = Done;
		currentWord = wordData;
    }

    protected virtual void ShowCard(WordData wordData, string levelName, int order) {
		stars = -1;
        WordCardManager.GetManager().SetUpWord(wordData, levelName, this);
        WordCardManager.GetManager().SetUpCard();
		//WordCardManager.GetManager().HideCard(null, true);
		WordCardManager.GetManager().ShowWordCard(order, () => { StartCard();});
    }

    public virtual void StartCard() {

    }

    public virtual void Retry() {
        StartCard();
    }

    public virtual void CardDone() {
		WordMaster.Instance.RecordStarAmount(currentWord.name, stars, (int)GetCardType());
        WordCardManager.GetManager().HideCard(()=>{ Done(stars); });
	}

	public virtual int GetStars() {
		return stars;
	}

    public virtual void SetStars(int stars) {
		//this.stars = Mathf.Max(this.stars, stars);
		// We provide the correct star amount, so we don't need to check for the max
		this.stars = stars;
    }

	protected abstract WordCardType GetCardType();
}
