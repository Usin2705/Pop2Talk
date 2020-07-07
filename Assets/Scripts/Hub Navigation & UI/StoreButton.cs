using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreButton : MonoBehaviour {

	UIButton button;
	LootBoxSettings myBox;

	IntCallback Clicked;
	int index;

	[SerializeField] Text priceText;
	[SerializeField] GameObject priceHolder;

	void Awake () {
		CheckButton();
	}

	void Start() {
		button.SubscribePress(Click);
	}

	public void SetUp(LootBoxSettings lootBox, int index, IntCallback OnClick) {
		CheckButton();
		myBox = lootBox;
		button.SetSprite(lootBox.picture);
		priceText.text = myBox.price.ToString();
		this.index = index;
		Clicked = OnClick;
	}
	

	void Click() {
		Clicked(index);
	}

	void CheckButton() {
		if (button == null)
			button = GetComponentInChildren<UIButton>();
	}

	public void TogglePrice(bool on) {
		priceHolder.SetActive(on);
	}

	public Transform GetBox() {
		return button.transform;
	}
}
