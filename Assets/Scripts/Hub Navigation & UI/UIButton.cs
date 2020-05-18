using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class UIButton : MonoBehaviour {

	[SerializeField] bool selectable;
	[SerializeField] Image insideIcon;
	[SerializeField] bool repressDeselects;
	[SerializeField] bool lockedGrayscale;

	bool selected;

	Button button;
	Text text;

	Sprite unlockedSprite;
	Sprite lockedSprite;

	List<Callback> PressCallbacks = new List<Callback>();
	List<Callback> SelectCallbacks = new List<Callback>();
	List<Callback> DeselectCallbacks = new List<Callback>();

	List<Callback> PressOneshots = new List<Callback>();
	List<Callback> SelectOneshots = new List<Callback>();
	List<Callback> DeselectOneshots = new List<Callback>();

	bool iterating;
	Dictionary<List<Callback>, List<Callback>> callbacksToUnsubscribe = new Dictionary<List<Callback>, List<Callback>>();
	Dictionary<List<Callback>, List<Callback>> callbacksToSubscribe = new Dictionary<List<Callback>, List<Callback>>();

	Color lockedColor = new Color(Color.gray.r, Color.gray.g, Color.gray.b, 0.7f);

	private void Awake() {
		button = GetComponent<Button>();
		button.onClick.AddListener(Press);
		text = GetComponentInChildren<Text>();
	}

	public void Press() {
		iterating = true;
		if (selectable) {
			if (!selected) {
				selected = true;
				CallSubscriptions(SelectCallbacks);
			} else {
				if (repressDeselects)
					Deselect();
			}
		}
		iterating = true;
		CallSubscriptions(PressCallbacks);
		CallSubscriptions(PressOneshots);
		PressOneshots.Clear();
		CheckSubscriptionChanges();
		iterating = false;
	}

	void CallSubscriptions(List<Callback> callbacks) {
		for (int i = callbacks.Count - 1; i >= 0; --i) {
			if (!iterating)
				break;
			callbacks[i]();
		}
	}

	public void Deselect() {
		selected = false;
		for (int i = DeselectCallbacks.Count - 1; i >= 0; --i) {
			DeselectCallbacks[i]();
		}
	}

	public void Destroy() {
		PressCallbacks.Clear();
		iterating = false;
		SelectCallbacks.Clear();
		DeselectCallbacks.Clear();
		ClearOneShots();
		//PoolMaster.Instance.Destroy(gameObject);
		Destroy(gameObject);
	}

	public void ClearOneShots() {
		PressOneshots.Clear();
		SelectOneshots.Clear();
		DeselectOneshots.Clear();
	}

	public void SetSprite(Sprite sprite, Sprite lockedSprite = null) {
		if (button == null)
			button = GetComponent<Button>();
		button.image.sprite = sprite;
		unlockedSprite = sprite;
		this.lockedSprite = lockedSprite;
	}

	public void SetText(string s) {
		if (text != null) {
			text.text = s;
			text.gameObject.SetActive(true);
			if (insideIcon != null)
				insideIcon.gameObject.SetActive(false);
		}
	}

	void CheckSubscriptionChanges() {
		foreach (List<Callback> list in callbacksToUnsubscribe.Keys) {
			foreach(Callback cb in callbacksToUnsubscribe[list])
				list.Remove(cb);
			callbacksToUnsubscribe[list].Clear();
		}

		foreach (List<Callback> list in callbacksToSubscribe.Keys) {
			foreach (Callback cb in callbacksToSubscribe[list])
				list.Add(cb);
			callbacksToSubscribe[list].Clear();
		}
	}

	public void SubscribePress(Callback PressCallback) {
		Subscribe(PressCallbacks, PressCallback);
	}

	public void SubscribeSelect(Callback SelectCallback) {
		Subscribe(SelectCallbacks, SelectCallback);
	}

	public void SubscribeDeselect(Callback DeselectCallback) {
		Subscribe(DeselectCallbacks, DeselectCallback);
	}

	void Subscribe(List<Callback> list, Callback callback) {
		if (!iterating)
			list.Add(callback);
		else {
			if (!callbacksToSubscribe.ContainsKey(list))
				callbacksToSubscribe.Add(list, new List<Callback>());
			callbacksToSubscribe[list].Add(callback);
		}
	}

	public void UnsubscribePress(Callback PressCallback) {
		Unsubscribe(PressCallbacks, PressCallback);
	}

	public void UnsubscribeSelect(Callback SelectCallback) {
		Unsubscribe(SelectCallbacks, SelectCallback);
	}

	public void UnsubscribeDeselect(Callback DeselectCallback) {
		Unsubscribe(DeselectCallbacks, DeselectCallback);
	}

	void Unsubscribe(List<Callback> list, Callback callback) {
		if (!iterating)
			list.Remove(callback);
		else {
			if (!list.Contains(callback) && callbacksToSubscribe.ContainsKey(list)) {
				callbacksToSubscribe[list].Remove(callback);
			} else {
				if (!callbacksToUnsubscribe.ContainsKey(list))
					callbacksToUnsubscribe.Add(list, new List<Callback>());
				callbacksToUnsubscribe[list].Add(callback);
			}
		}
	}

	public void OneshotPress(Callback PressCallback) {
		PressOneshots.Add(PressCallback);
	}

	public void RemoveOneshotPress(Callback PressCallback) {
		PressOneshots.Remove(PressCallback);
	}

	public void SetLocked(bool locked) {
		if (lockedSprite != null)
			button.image.sprite = (locked) ? lockedSprite : unlockedSprite;
		if (lockedGrayscale)
			button.image.color = (locked) ? lockedColor : Color.white;
		button.interactable = !locked;
	}

	public void SetIcon(Sprite icon) {
		if (insideIcon != null) {
			insideIcon.sprite = icon;
			text.gameObject.SetActive(false);
			insideIcon.gameObject.SetActive(true);
		}
	}
}