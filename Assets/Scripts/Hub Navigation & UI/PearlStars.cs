using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PearlStars : MonoBehaviour {

	UIButton button;
	string myWord;
	WordPearlView view;
	int stars;

	[SerializeField] Transform starHolder;

	void Awake () {
		button = GetComponentInChildren<UIButton>();
	}

	void Start() {
		button.SubscribePress(Click);
	}

	public void SetUp(string word, WordPearlView view) {
		myWord = word;
		this.view = view;
		button.SetSprite(WordMaster.Instance.StringToWordData(word).picture);
	}

	public void SetStars(int stars) {
		this.stars = Mathf.Max(this.stars, stars);
		for (int i = 0; i < starHolder.childCount; ++i) {
			starHolder.GetChild(i).gameObject.SetActive(i < stars);
		}
	}

	void Click() {
		view.ShowCard(myWord);
	}
}
