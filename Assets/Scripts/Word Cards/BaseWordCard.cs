using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseWordCard : Overlay {

	public abstract void SetPicture(Sprite sprite);
	public abstract void ToggleMic(bool on);
	public abstract void ToggleButtons(bool on);
	public abstract Coroutine SetStars(int amount, float perStarDuration = 0);
	public abstract void SetMemory(bool on);
	public abstract void VisualizeAudio(float[] samples);
	public abstract Coroutine ShowCard(float duration);
	public abstract void HideCard(float duration, Callback Done);
	public abstract void HideInstantly();
	public abstract void RevealInstantly();
	public abstract void SetOfflineStars();
	public abstract void SetOnlineStars();
	public abstract void StopCard();
	public abstract Coroutine FinishingAnimation();
	public abstract Coroutine StartingAnimation();

	protected IEnumerator ToggleStars(Image[] starImages, float bulgeSize, int amount, float perStarDuration) {
		for (int i = 0; i < starImages.Length; ++i) {
			starImages[i].gameObject.SetActive(amount > i);
			if (amount > i) {
				float a = 0;
				Vector3 startScale = starImages[i].transform.localScale;
				while (a < 1) {
					if (perStarDuration > 0)
						a += Time.deltaTime / perStarDuration;
					else
						a = 1;
					starImages[i].transform.localScale = Vector3.Lerp(startScale * bulgeSize, startScale, a);
					if (perStarDuration > 0)
						yield return null;
				}
			}
		}
	}

	protected IEnumerator HideStars(Image[] starImages, float bulgeSize, int amount, float perStarDuration) {
		for (int i = amount-1; i >= 0; --i) {
			float a = 0;
			Vector3 startScale = starImages[i].transform.localScale;
			while (a < 1) {
				if (perStarDuration > 0)
					a += Time.deltaTime / perStarDuration;
				else
					a = 1;
				starImages[i].transform.localScale = Vector3.Lerp(startScale * bulgeSize, Vector3.zero, a);
				if (perStarDuration > 0)
					yield return null;
			}
			starImages[i].gameObject.SetActive(false);
			starImages[i].transform.localScale = startScale;
		}
	}

	public void Retry() {
		WordCardManager.GetManager().Retry();
	}

	public void Continue() {
		WordCardManager.GetManager().Continue();
	}
}
