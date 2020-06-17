using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIButton))]
public class ButtonEffects : MonoBehaviour {

	[SerializeField] float selectedScale = 1;
	[SerializeField] float deselectedScale = 1;
	[SerializeField] float scaleDuration;
	[SerializeField] float wobbleAmount;
	[SerializeField] float wobbleSpeed;
	[SerializeField] bool sound = true;

	UIButton uib;
	float currentScale = 1;
	float scaleSpeed;
	float targetScale = 1;

	float wobbleTimer;

	void Awake() {
		uib = GetComponent<UIButton>();
		targetScale = deselectedScale;
		Scale();
		wobbleTimer = Random.Range(0, Mathf.PI * 2);
	}

	void Start() {
		uib.SubscribeSelect(Select);
		uib.SubscribeDeselect(Deselect);
		uib.SubscribePress(Press);
		if (uib.Selected) {
			targetScale = selectedScale;
			Scale(true);
		}
	}

	void Update() {
		if (targetScale != currentScale)
			Scale();
		if (wobbleAmount != 0 && wobbleSpeed != 0)
			Wobble();
	}

	void Press() {
		if (sound)
			AudioMaster.Instance.Play(this, SoundEffectManager.GetManager().GetButtonSound());
	}

	void Select() {
		if (selectedScale == deselectedScale)
			return;
		targetScale = selectedScale;
		if (scaleDuration != 0)
			scaleSpeed = (selectedScale - deselectedScale) / scaleDuration;
		else {
			Scale();
		}
	}

	void Scale(bool ignoreDuration = false) {
		if (scaleDuration > 0 && scaleSpeed > 0 && !ignoreDuration)
			currentScale = Mathf.MoveTowards(currentScale, targetScale, scaleSpeed * Time.deltaTime);
		else {
			currentScale = targetScale;
		}

		transform.localScale = currentScale * Vector3.one;
	}

	void Deselect() {
		if (selectedScale == deselectedScale)
			return;
		targetScale = deselectedScale;
		if (scaleDuration != 0)
			scaleSpeed = (selectedScale - deselectedScale) / scaleDuration;
		else {
			Scale();
		}
	}

	void Wobble() {
		wobbleTimer += Time.deltaTime * wobbleSpeed;
		transform.GetChild(0).localPosition = Vector3.up * Mathf.Sin(wobbleTimer) * wobbleAmount;

	}

    public void SetWobble(float amount, float speed)
    {
        wobbleAmount = amount;
        wobbleSpeed = speed;
    }
}
