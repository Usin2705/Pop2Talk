using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIHandler : MonoBehaviour {

    [SerializeField]
    Text stars;
    [SerializeField]
    Text trackedValue;
	[SerializeField]
	Image trackedIcon;
	[SerializeField]
    Text cardsLeft;
    [SerializeField]
    MachineCard machineCard;
    [SerializeField]
    float numberSpeed = 5000;
    [SerializeField]
    float minNumberDuration = 0.2f;
    [SerializeField]
    float maxNumberDuration = 3f;
    [SerializeField]
    GameObject uiHolder;
    [SerializeField]
    GameObject backButton;

    float bulgeTime = -1;
    float bulgeDuration = 0.5f;
    float bulgeAmount = 1.33f;
	
    Dictionary<Text, Coroutine> currentRoutines = new Dictionary<Text, Coroutine>();

	public void SetCardBar(bool on) {
		machineCard.SetBarShowAsHideSlot(on);
	}

    public void ToggleBack(bool on) {
        if (backButton != null)
            backButton.SetActive(on);
    }

	public void SetTrackedIcon(Sprite icon) {
		trackedIcon.sprite = icon;
	}

    public void SetStars(int starAmount, bool instant = false) {
        ChangeText(stars, starAmount, instant);
    }

    public void SetTrackedValue(int score, bool instant = false) {
        ChangeText(trackedValue, score, instant);
    }

    public void SetProgress(float ratio) {
		machineCard.SetProgress(ratio);
    }

    public void SetCardsLeft(int cards, bool instant = false) {
        ChangeText(cardsLeft, cards, instant);
    }

    public void ToggleUI(bool on) {
        uiHolder.SetActive(on);
    }

    void ChangeText(Text t, int i, bool instant) {
        if (t != null) {
            if (currentRoutines.ContainsKey(t)) {
                if (currentRoutines[t] != null)
                    StopCoroutine(currentRoutines[t]);
            } else {
                currentRoutines[t] = null;
            }
            if (instant)
                t.text = i.ToString();
            else {
                currentRoutines[t] = StartCoroutine(TextRoutine(t, i));
            }
        }
    }

    IEnumerator TextRoutine(Text t, float target) {
        int start = 0;
        while (bulgeTime + bulgeDuration > Time.time)
            yield return null;
        System.Int32.TryParse(t.text, out start);
        float v = Mathf.Clamp(numberSpeed, Mathf.Abs(target - start) / maxNumberDuration, Mathf.Abs(target - start) / minNumberDuration);
        float motion = start;
        while (!Mathf.Approximately(motion, target)) {
            motion = Mathf.MoveTowards(motion, target, v * Time.deltaTime);
            t.text = ((int)motion).ToString();
            yield return null;
        }
        t.text = ((int)target).ToString();
        currentRoutines[t] = null;
    }

    void BulgeText(Text t, int i) {
        if (t != null) {
            if (currentRoutines.ContainsKey(t)) {
                if (currentRoutines[t] != null)
                    StopCoroutine(currentRoutines[t]);
            } else {
                currentRoutines[t] = null;
            }
            t.text = i.ToString();
            StartCoroutine(Bulge(t));
        }
    }

    IEnumerator Bulge(Text t) {
        Vector3 start = t.transform.localScale;
        bulgeTime = Time.time;
        float a = 0;
        while (a < 1) {
            a += Time.deltaTime / bulgeDuration;
            t.transform.localScale = Vector3.Lerp(start * bulgeAmount, start, a);
            yield return null;
        }
    }
}
