using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WordCard : BaseWordCard {
  
    [SerializeField] Image wordImage;
	[SerializeField] Image frontBackground;
	[SerializeField] Image backBackground;
	[SerializeField] RectTransform cardHolder;
	[SerializeField] RectTransform frontSide;
	[SerializeField] RectTransform backSide;
	[SerializeField] Image flagOne;
    [SerializeField] Image flagTwo;
    [SerializeField] Image micPic;
	[SerializeField] Image micDisabled;
	[SerializeField] VolumeBars visualizer;
    [SerializeField] Text wordSpelling;
    [SerializeField] Text advancedFeedback;
    [SerializeField] Material grayscale;
    [SerializeField] Image[] starImages;
    [SerializeField] GameObject retryButton;
    [SerializeField] GameObject continueButton;
    [SerializeField] Image waitBar;
	[SerializeField] Sprite quizFront;
	[SerializeField] Sprite quizBack;
	[SerializeField] AudioInstance waitAudio;
	[SerializeField] RectTransform cardHideSlot;
	[SerializeField] RectTransform cardShowSlot;
	[SerializeField] RectTransform retryShowSlot;
	[SerializeField] RectTransform retryHideSlot;
	[SerializeField] RectTransform continueShowSlot;
	[SerializeField] RectTransform continueHideSlot;
	[SerializeField] Image fadeCurtain;
	[SerializeField] Color offlineColor;

	float starBulgeSize = 1.33f;
	float buttonDuration = 0.25f;
    Color curtainColor;

	Sprite normalFront;
	Sprite normalBack;

    private void Awake() {
        curtainColor = fadeCurtain.color;
		normalFront = frontBackground.sprite;
		normalBack = backBackground.sprite;
    }

    public override void SetPicture(Sprite sprite) {
        wordImage.sprite = sprite;
    }

    public override void SetSpelling(string spelling) {
        wordSpelling.text = spelling;
    }

    public override void SetFlagOneOn(bool on) {
        SetFlagOn(flagOne,on);
    }

    public override void SetFlagTwoOn(bool on) {
        SetFlagOn(flagTwo, on);
    }

    public override void ToggleFeedback(bool on) {
        advancedFeedback.gameObject.SetActive(on);
    }

    public override void ToggleMic(bool on) {
        micPic.gameObject.SetActive(on);
		micDisabled.gameObject.SetActive(!on);
	}

    public override Coroutine SetStars(int amount, float perStarDuration = 0) {
        return StartCoroutine(ToggleStars(starImages, starBulgeSize, amount, perStarDuration));
    }

    void SetFlagOn(Image flag,bool on) {
        flag.material = (on) ? null : grayscale;
    }

    public override Coroutine ShowCard(float duration) {
        return StartCoroutine(IntroSequence(duration));
    }

    IEnumerator IntroSequence(float duration) {
        float a = 0;
        Color start = new Color(curtainColor.r,curtainColor.g,curtainColor.b, 0);
		AudioMaster.Instance.Play(this, SoundEffectManager.GetManager().GetWordCardEnterSound());
        while (a < 1) {
            if (duration > 0)
                a += Time.deltaTime / (duration / 3);
            else
                a = 1;
            fadeCurtain.color = Color.Lerp(start, curtainColor,a);
            cardHolder.position = Vector3.Lerp(cardHideSlot.position, cardShowSlot.position, a);
            if (duration > 0)
                yield return null;
		}
		a = 0;
		yield return new WaitForSeconds(duration / 3);
		while (a < 1) {
			if (duration > 0)
				a += Time.deltaTime / (duration / 6);
			else
				a = 1;
			cardHolder.rotation = Quaternion.Euler(new Vector3(0, Mathf.Lerp(0, 90f, a), 0));
			if (duration > 0)
				yield return null;
		}
		a = 0;
		backSide.gameObject.SetActive(false);
		frontSide.gameObject.SetActive(true);
		while (a < 1) {
			if (duration > 0)
				a += Time.deltaTime / (duration / 6);
			else
				a = 1;
			cardHolder.rotation = Quaternion.Euler(new Vector3(0, Mathf.Lerp(90f, 0, a), 0));
			if (duration > 0)
				yield return null;
		}
	}

	public override void RevealInstantly() {
		fadeCurtain.color = curtainColor;
		cardHolder.position = cardShowSlot.position;
		backSide.gameObject.SetActive(false);
		frontSide.gameObject.SetActive(true);
	}

    public override void ToggleButtons(bool on) {
		StartCoroutine(ButtonToggleRoutine(continueButton.transform, continueHideSlot, continueShowSlot, on));
		StartCoroutine(ButtonToggleRoutine(retryButton.transform, retryHideSlot, retryShowSlot, on));
	}

	IEnumerator ButtonToggleRoutine(Transform button, Transform hideSlot, Transform showSlot, bool on) {
		float a = 0;
		if (on)
			button.gameObject.SetActive(true);
		while (a < 1) {
			a += Time.deltaTime / buttonDuration;
			button.position = Vector3.Lerp(hideSlot.position, showSlot.position, (on) ? a : (1 - a));
			yield return null;
		}
		if (!on)
			button.gameObject.SetActive(false);
	}

	public override void SetQuiz(bool on) {
        frontBackground.sprite = (on) ? quizFront : normalFront;
		backBackground.sprite = (on) ? quizBack : normalBack;
		//visualizer.SetQuiz(on);
	}

    public override void Wait(float duration) {
        StartCoroutine(WaitRoutine(duration));
    }

    IEnumerator WaitRoutine(float duration) {
        float a = 0;
        float tickTimer = 0;
        waitBar.gameObject.SetActive(true);
        while(a < 1) {
            a += Time.deltaTime / duration;
            tickTimer += Time.deltaTime;
            waitBar.fillAmount = 1-a;
            if (tickTimer >= 0.5f) {
                AudioMaster.Instance.Play(this, waitAudio);
                tickTimer = 0;
            }
            yield return null;
        }
        waitBar.gameObject.SetActive(false);
    }

    public override void VisualizeAudio(float[] samples) {
        visualizer.Visualize(samples);
    }

	public override void HideCard(float duration, Callback Done) {
		ToggleButtons(false);
		StartCoroutine(HideRoutine(duration, Done));
	}

	public override void HideInstantly() {
		cardHolder.position = cardHideSlot.position;
		fadeCurtain.color = new Color(curtainColor.r, curtainColor.g, curtainColor.b, 0);
		StopCard();
	}

	IEnumerator HideRoutine(float duration, Callback Done) {
		float a = 0;
        Color target = new Color(curtainColor.r, curtainColor.g, curtainColor.b,0);
		while (a < 1) {
			if (duration > 0)
				a += Time.deltaTime / duration;
			else
				a = 1;
            fadeCurtain.color = Color.Lerp(curtainColor, target, a);
			cardHolder.position = Vector3.Lerp(cardShowSlot.position, cardHideSlot.position, a);
			if (duration > 0)
				yield return null;
		}
		StopCard();
		if (Done != null)
			Done();
	}

	public override void StopCard() {
		StopAllCoroutines();
		SetQuiz(false);
		backSide.gameObject.SetActive(true);
		frontSide.gameObject.SetActive(false);
		continueButton.gameObject.SetActive(false);
		retryButton.gameObject.SetActive(false);
		gameObject.SetActive(false);
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

	public override Coroutine FinishingAnimation() {
		return null;
	}

	public override Coroutine StartingAnimation() {
		return null;
	}
}
