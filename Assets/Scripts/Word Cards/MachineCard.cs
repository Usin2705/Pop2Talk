using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MachineCard : BaseWordCard {

	[SerializeField] Image wordImage;
	[SerializeField] RectTransform cardHolder;
	[SerializeField] Image micPic;
	[SerializeField] Image micDisabled;
	[SerializeField] VolumeBars volumeBars;
	[SerializeField] Image progressBar;
	[SerializeField] Image[] starImages;
	[SerializeField] Sprite quizMic;
	[SerializeField] Sprite quizMicOff;
	[SerializeField] UIButton continueButton;
	[SerializeField] UIButton retryButton;
	[SerializeField] RectTransform cardHideSlot;
	[SerializeField] RectTransform cardShowSlot;
	[SerializeField] RectTransform barShowSlot;
	[SerializeField] RectTransform cover;
	[SerializeField] RectTransform coverHideSlot;
	[SerializeField] RectTransform coverShowSlot;
	[SerializeField] PearlStars pearl;
	[SerializeField] RectTransform pearlStartSlot;
	[SerializeField] RectTransform pearlEndSlot;
	[SerializeField] Image fadeCurtain;
	[SerializeField] Color offlineColor;

	float starBulgeSize = 1.33f;
	Color curtainColor;

	Sprite startMic;
	Sprite startMicOff;

	bool showingBar;
	RectTransform currentHideSlot;

	bool initialized = false;

	int starAmount;

	private void Awake() {
		Initialize();
		continueButton.SubscribePress(Continue);
		retryButton.SubscribePress(Retry);
		ToggleButtons(false);

	}

	void Initialize() {
		if (initialized)
			return;
		curtainColor = fadeCurtain.color;
		startMic = micPic.sprite;
		startMicOff = micDisabled.sprite;
		currentHideSlot = cardHideSlot;
		SetBarShowAsHideSlot(false);
		initialized = true;
	}

	public void SetBarShowAsHideSlot(bool on) {
		if (on && !gameObject.activeSelf)
			gameObject.SetActive(true);
		showingBar = on;
		currentHideSlot = (on) ? barShowSlot : cardHideSlot;
		HideInstantly();
	}

	public override void SetPicture(Sprite sprite) {
		wordImage.sprite = sprite;
	}

	public override void ToggleMic(bool on) {
		micPic.gameObject.SetActive(on);
		volumeBars.gameObject.SetActive(on);
	}

	public override Coroutine SetStars(int amount, float perStarDuration = 0) {
		starAmount = amount;
		return StartCoroutine(ToggleStars(starImages, starBulgeSize, amount, perStarDuration));
	}

	public override Coroutine ShowCard(float duration) {
		gameObject.SetActive(true);
		return StartCoroutine(IntroSequence(duration));
	}

	IEnumerator IntroSequence(float duration) {
		fadeCurtain.gameObject.SetActive(true);
		float a = 0;
		Color start = new Color(curtainColor.r, curtainColor.g, curtainColor.b, 0);
		AudioMaster.Instance.Play(this, SoundEffectManager.GetManager().GetWordCardEnterSound());
		while (a < 1) {
			if (duration > 0)
				a += Time.deltaTime / (duration / 2);
			else
				a = 1;
			fadeCurtain.color = Color.Lerp(start, curtainColor, a);
			cardHolder.position = Vector3.Lerp(currentHideSlot.position, cardShowSlot.position, a);
			SetProgress(1 - a);
			if (duration > 0)
				yield return null;
		}
		yield return new WaitForSeconds(duration / 2f);
	}

	public override void RevealInstantly() {
		gameObject.SetActive(true);
		Debug.Log(gameObject.activeSelf);
		fadeCurtain.gameObject.SetActive(true);
		fadeCurtain.color = curtainColor;
		cardHolder.position = cardShowSlot.position;
	}

	public override void ToggleButtons(bool on) {
		continueButton.gameObject.SetActive(on);
		retryButton.gameObject.SetActive(on);
	}

	public override void SetMemory(bool on) {
		micPic.sprite = (on) ? quizMic : startMic;
		micDisabled.sprite = (on) ? quizMicOff : startMicOff;
		volumeBars.SetQuiz(on);
	}

	public override void VisualizeAudio(float[] samples) {
		volumeBars.Visualize(samples);
	}

	public override void HideCard(float duration, Callback Done, bool newHighScore, WordData word) {
		fadeCurtain.gameObject.SetActive(false);
		ToggleButtons(false);
		StartCoroutine(HideRoutine(duration, Done, newHighScore, word));
	}

	public override void HideInstantly() {
		cardHolder.position = currentHideSlot.position;
		cover.position = coverShowSlot.position;
		fadeCurtain.color = new Color(curtainColor.r, curtainColor.g, curtainColor.b, 0);
		fadeCurtain.gameObject.SetActive(false);
		StopCard();
	}

	IEnumerator HideRoutine(float duration, Callback Done, bool newHighScore, WordData word) {
		float a = 0;
		yield return StartCoroutine(HideStars(starImages, starBulgeSize, starAmount, 0.4f));
		yield return new WaitForSeconds(duration / 4);
		if (newHighScore) {
			AudioMaster.Instance.Play(this, SoundEffectManager.GetManager().GetPearlSound());
			RectTransform rect = pearl.GetComponent<RectTransform>();
			pearl.SetUp(word.name, null);
			pearl.SetStars(WordMaster.Instance.GetHighScore(word.name));
			while (a < 1) {
				if (duration > 0)
					a += Time.deltaTime / (duration / 4);
				else
					a = 1;
				rect.position = Vector3.Lerp(pearlStartSlot.position, pearlEndSlot.position, a);
				if (duration > 0)
					yield return null;
			}
			a = 0;
			yield return new WaitForSeconds(duration / 4);
		}
		Color target = new Color(curtainColor.r, curtainColor.g, curtainColor.b, 0);
		while (a < 1) {
			if (duration > 0)
				a += Time.deltaTime / (duration / 4);
			else
				a = 1;
			fadeCurtain.gameObject.SetActive(false);
			fadeCurtain.color = Color.Lerp(curtainColor, target, a);
			cardHolder.position = Vector3.Lerp(cardShowSlot.position, currentHideSlot.position, a);
			if (duration > 0)
				yield return null;
		}
		StopCard();
		Done?.Invoke();
	}

	public override Coroutine FinishingAnimation() {
		return StartCoroutine(MoveCoverRoutine(0.25f, coverHideSlot, coverShowSlot));
	}

	public override Coroutine StartingAnimation() {
		return StartCoroutine(MoveCoverRoutine(0.25f, coverShowSlot, coverHideSlot));
	}

	IEnumerator MoveCoverRoutine(float duration, RectTransform current, RectTransform target) {
		float a = 0;
		while (a < 1) {
			if (duration > 0)
				a += Time.deltaTime / (duration);
			else
				a = 1;
			cover.position = Vector3.Lerp(current.position, target.position, a);
			if (duration > 0)
				yield return null;
		}
	}

	public override void StopCard() {
		foreach (Image i in starImages)
			i.gameObject.SetActive(false);
		starAmount = 0;
		SetProgress(0);
		StopAllCoroutines();
		SetMemory(false);
		ToggleButtons(false);
		pearl.GetComponent<RectTransform>().position = pearlStartSlot.position;
		gameObject.SetActive(showingBar);
	}

	public override void SetOfflineStars() {
		foreach (Image i in starImages) {
			i.color = offlineColor;
		}
	}

	public override void SetOnlineStars() {
		foreach (Image i in starImages) {
			i.color = Color.white;
		}
	}

	public void SetProgress(float ratio) {
		progressBar.fillAmount = ratio;
	}
}
