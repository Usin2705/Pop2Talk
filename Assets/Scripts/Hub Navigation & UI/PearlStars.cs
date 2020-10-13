using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PearlStars : MonoBehaviour {

	UIButton button;
	string myWord;
	WordPearlView view;

	[SerializeField] Transform starHolder = null;
	[SerializeField] Image bottom = null;
	[SerializeField] Sprite[] sprites = null;
	[SerializeField] Color[] colors = null;

	public int Stars { get; protected set; }

	void Awake() {
		CheckButton();
	}

	void Start() {
		button.SubscribePress(Click);
	}

	public void SetUp(string word, WordPearlView view) {
		myWord = word;
		this.view = view;
		CheckButton();
		button.SetSprite(WordMaster.Instance.StringToWordData(word).picture);
	}

	public void SetStars(int stars) {
		CheckButton();
		Stars = Mathf.Max(this.Stars, stars);
		stars = Stars;
		bottom.sprite = sprites[Mathf.Max(0, stars - 2)];
		button.SetSpriteTint(colors[Mathf.Max(0, stars - 2)]);
		for (int i = 0; i < starHolder.childCount; ++i) {
			starHolder.GetChild(i).gameObject.SetActive(i < stars);
		}
	}

	public void SetAnimatedStars(int stars, float duration) {
		StartCoroutine(StarAnimation(stars, duration));
	}

	IEnumerator StarAnimation(int stars, float duration) {
		float a = 0;
		while (a < 1) {
			if (duration == 0)
				a = 1;
			else
				a += Time.deltaTime / duration * 2;
			a = Mathf.Clamp01(a);
			transform.localScale = Vector3.one * (1 - a);
			if (duration > 0)
				yield return null;
		}
		SetStars(stars);
		a = 0;
		while (a < 1) {
			if (duration == 0)
				a = 1;
			else
				a += Time.deltaTime / duration * 2;
			a = Mathf.Clamp01(a);
			transform.localScale = Vector3.one * a;
			if (duration > 0)
				yield return null;
		}
	}

	void CheckButton() {
		if (button == null)
			button = GetComponentInChildren<UIButton>();
	}

	void Click() {
		view.ShowCard(myWord);
	}
}
