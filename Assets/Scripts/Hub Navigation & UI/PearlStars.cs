using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PearlStars : MonoBehaviour {

	UIButton button;
	string myWord;
	WordPearlView view;
	int stars;

	[SerializeField] Transform starHolder;
	[SerializeField] Image bottom;
	[SerializeField] Sprite[] sprites;
	[SerializeField] Color[] colors;

	void Awake () {
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
		this.stars = Mathf.Max(this.stars, stars);
		stars = this.stars;
		bottom.sprite = sprites[Mathf.Max(0,stars - 2)];
		button.SetSpriteTint(colors[Mathf.Max(0, stars - 2)]);
		for (int i = 0; i < starHolder.childCount; ++i) {
			starHolder.GetChild(i).gameObject.SetActive(i < stars);
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
