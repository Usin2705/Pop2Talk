﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeatCardHandler : BaseWordCardHandler {
	
	public override void ShowCard(WordData wordData, string levelName, int order, IntCallback Done) {
		base.ShowCard(wordData, levelName, order, Done);
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
            yield return WordCardManager.GetManager().StartCoroutine(WordCardManager.GetManager().SayWord());
            yield return WordCardManager.GetManager().StartCoroutine(WordCardManager.GetManager().RecordAndPlay(phaseGap, "RepeatChallenge"));
            yield return new WaitForSeconds(phaseGap*3);
            yield return WordCardManager.GetManager().StartCoroutine(WordCardManager.GetManager().SayWord());
            WordCardManager.GetManager().StartCoroutine(WordCardManager.GetManager().GiveStars(phaseGap));
        } else {
            yield return new WaitForSeconds(phaseGap);            
            yield return WordCardManager.GetManager().StartingAnimation();
            yield return WordCardManager.GetManager().StartCoroutine(WordCardManager.GetManager().SayWord());
            yield return WordCardManager.GetManager().StartCoroutine(WordCardManager.GetManager().RecordAndPlay(phaseGap, "RepeatChallenge", false));
            yield return new WaitForSeconds(phaseGap*3);
            yield return WordCardManager.GetManager().StartCoroutine(WordCardManager.GetManager().SayWord());
            WordCardManager.GetManager().StartCoroutine(WordCardManager.GetManager().SkipStars());
        }
    }

	protected override WordCardType GetCardType() {
		return WordCardType.Repeat;
	}
}
