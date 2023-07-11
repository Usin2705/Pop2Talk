using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryCardHandler : BaseWordCardHandler {

	public override void ShowCard(WordData wordData, string levelName, int order, IntCallback Done) {
		base.ShowCard(wordData, levelName, order, Done);
		WordCardManager.GetManager().SetMemory(true);
		ShowCard(wordData, levelName, order);
	}

	public override void StartCard() {
        base.StartCard();
        WordCardManager.GetManager().StartCoroutine(CardRoutine());
    }

    public override void Retry() {
        WordCardManager.GetManager().SetUpCard();
        base.Retry();
    }

    IEnumerator CardRoutine() {		
        if (WordMaster.Instance.is_feedback) {		
			yield return new WaitForSeconds(phaseGap);
			yield return WordCardManager.GetManager().StartingAnimation();
			yield return WordCardManager.GetManager().StartCoroutine(WordCardManager.GetManager().RecordAndPlay(phaseGap, "MemoryChallenge"));
			yield return new WaitForSeconds(phaseGap);
			WordCardManager.GetManager().StartCoroutine(WordCardManager.GetManager().GiveStars(phaseGap));
		} else {
			yield return new WaitForSeconds(phaseGap);
			yield return WordCardManager.GetManager().StartingAnimation();
			yield return WordCardManager.GetManager().StartCoroutine(WordCardManager.GetManager().RecordAndPlay(phaseGap, "MemoryChallenge", false));
			yield return new WaitForSeconds(phaseGap);
			WordCardManager.GetManager().StartCoroutine(WordCardManager.GetManager().SkipStars());
		}
    }

	protected override WordCardType GetCardType() {
		return WordCardType.Memory;
	}
}
