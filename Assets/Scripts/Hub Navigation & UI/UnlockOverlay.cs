﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnlockOverlay : Overlay {

	[SerializeField] GameObject screen = null;
	[SerializeField] Image whiteCurtain = null;
	[SerializeField] Image midImage = null;
	[SerializeField] Image topImage = null;
	[SerializeField] Image botImage = null;
	[SerializeField] Image textImage = null;
	[SerializeField] Text amountText = null;
	[SerializeField] UIButton button = null;

	Callback ClickCallback;

	public static UnlockOverlay Instance {
		get; protected set;
	}

	void Awake() {
		Instance = this;
		button.SubscribePress(ButtonPressed);
	}

	public void ShowUnlock(int order, Sprite sprite, Callback click) {
		botImage.gameObject.SetActive(false);
		topImage.gameObject.SetActive(false);
		ShowUnlock(order, sprite, click, midImage);
	}

	public void ShowTopUnlock(int order, Sprite sprite, Callback click) {
		botImage.gameObject.SetActive(false);
		midImage.gameObject.SetActive(false);
		ShowUnlock(order, sprite, click, topImage);
	}

	public void ShowBotUnlock(int order, Sprite sprite, Callback click) {
		topImage.gameObject.SetActive(false);
		midImage.gameObject.SetActive(false);
		ShowUnlock(order, sprite, click, botImage);
	}

	void ShowUnlock(int order, Sprite sprite, Callback Click, Image image) {
		SetOrder(order);
		image.gameObject.SetActive(true);
		textImage.gameObject.SetActive(false);
		amountText.gameObject.SetActive(false);
		image.sprite = sprite;
		ClickCallback = Click;
		StartCoroutine(ShowUnlockRoutine());
	}

	public void ShowUnlock(int order, Sprite sprite, string text, Callback Click) {
		SetOrder(order);
		topImage.gameObject.SetActive(false);
		midImage.gameObject.SetActive(false);
		botImage.gameObject.SetActive(false);
		textImage.gameObject.SetActive(true);
		amountText.gameObject.SetActive(true);
		amountText.text = text;
		textImage.sprite = sprite;
		ClickCallback = Click;
		StartCoroutine(ShowUnlockRoutine());
	}

	void ButtonPressed() {
		ClickCallback?.Invoke();
		HideScreen();
	}

	IEnumerator ShowUnlockRoutine() {
		InputManager.GetManager().SendingInputs = false;
		screen.SetActive(true);
		if (!DebugMaster.Instance.skipTransitions) {
			whiteCurtain.gameObject.SetActive(true);
			yield return new WaitForSeconds(0.5f);
			float a = 0;
			while (a < 1) {
				a += Time.deltaTime / 0.5f;
				whiteCurtain.color = new Color(1, 1, 1, 1 - a);
				yield return null;
			}
		}
		whiteCurtain.gameObject.SetActive(false);
		whiteCurtain.color = Color.white;
		InputManager.GetManager().SendingInputs = true;
	}

	void HideScreen() {
		screen.SetActive(false);
	}
}
