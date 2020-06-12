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
	[SerializeField] Sprite defaultPearl;
	[SerializeField] Sprite bronze;
	[SerializeField] Sprite silver;
	[SerializeField] Sprite gold;

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
		stars = this.stars;
		switch (stars) {
			case 5:
				bottom.sprite = gold;
				break;
			case 4:
				bottom.sprite = silver;
				break;
			case 3:
				bottom.sprite = bronze;
				break;
			default:
				bottom.sprite = defaultPearl;
				break;

		}
		for (int i = 0; i < starHolder.childCount; ++i) {
			starHolder.GetChild(i).gameObject.SetActive(i < stars);
		}
	}

	void Click() {
		view.ShowCard(myWord);
	}
}
