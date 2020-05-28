using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizCardHandler : BaseWordCardHandler {

	public override void ShowCard(WordData wordData, string levelName, Language targetLanguage, Language nativeLanguage, int order, IntCallback Done) {
		base.ShowCard(wordData, levelName, targetLanguage, nativeLanguage, order, Done);
		WordCardManager.GetManager().SetQuiz(true);
		ShowCard(wordData, levelName, order);
	}

	public override void StartCard() {
        base.StartCard();
        WordCardManager.GetManager().StartCoroutine(CardRoutine());
    }

    public override void Retry() {
        WordCardManager.GetManager().SetUpCard(targetLanguage);
        base.Retry();
    }

    IEnumerator CardRoutine() {
		yield return new WaitForSeconds(phaseGap);
		yield return WordCardManager.GetManager().StartingAnimation();
		yield return WordCardManager.GetManager().StartCoroutine(WordCardManager.GetManager().QuizPrompt(nativeLanguage, targetLanguage));
		if (nativeLanguage != targetLanguage) {
			yield return new WaitForSeconds(phaseGap);
			WordCardManager.GetManager().SetFlags(true, false);
			yield return WordCardManager.GetManager().StartCoroutine(WordCardManager.GetManager().SayWord(nativeLanguage));
			WordCardManager.GetManager().SetFlags(false, false);
		}
        //yield return new WaitForSeconds(phaseGap);
		yield return WordCardManager.GetManager().StartCoroutine(WordCardManager.GetManager().RecordAndPlay(phaseGap, "MemoryChallenge"));
        yield return new WaitForSeconds(phaseGap);
        WordCardManager.GetManager().StartCoroutine(WordCardManager.GetManager().GiveStars(phaseGap));
    }
}
