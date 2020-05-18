using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseWordCardHandler {

    protected static float phaseGap = 0.66f;

	protected Language nativeLanguage;
	protected Language targetLanguage;

	protected IntCallback Done;

	protected int stars = -1;

	public virtual void ShowCard(WordData wordData, string levelName, Language targetLanguage, Language nativeLanguage, int order, IntCallback Done) {
		this.nativeLanguage = nativeLanguage;
		this.targetLanguage = targetLanguage;
		this.Done = Done;
    }

    protected virtual void ShowCard(WordData wordData, string levelName, int order) {
		stars = -1;
        WordCardManager.GetManager().SetUpWord(wordData, levelName, this);
        WordCardManager.GetManager().SetUpCard(targetLanguage);
        WordCardManager.GetManager().ShowWordCard(order, () => { StartCard();});
    }

    public virtual void StartCard() {

    }

    public virtual void Retry() {
        StartCard();
    }

    public virtual void CardDone() {
        WordCardManager.GetManager().HideCard(()=>{ Done(stars); });
	}

	public virtual int GetStars() {
		return stars;
	}

    public virtual void SetStars(int stars) {
		this.stars = Mathf.Max(this.stars, stars);
    }
}
