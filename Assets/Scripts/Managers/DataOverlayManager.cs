using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataOverlayManager : Overlay, IFingerPointable {

	static DataOverlayManager dom;
	[SerializeField] float showDuration = 0;
	[SerializeField] GameObject root = null;
	[SerializeField] Image curtain = null;
	[SerializeField] RectTransform overlayRoot = null;
	[SerializeField] RectTransform iconStringRoot = null;
	[SerializeField] RectTransform buttonRoot = null;
	[SerializeField] GameObject closeButton = null;
	[SerializeField] Text titleShort = null;
	[SerializeField] Text titleLong = null;
	[SerializeField] GameObject iconStringPrefab = null;
	[SerializeField] UIButton buttonPrefab = null;
    [SerializeField] UIButton closeUIButton = null;

    List<GameObject> iconStrings = new List<GameObject>();
	List<UIButton> uiButtons = new List<UIButton>();

	bool interrupt = false;
	bool routineActive = false;

	public bool Active { get { return root.activeSelf; } }

    void Start()
    {
        closeUIButton.SubscribePress(Close);
    }

	public static DataOverlayManager GetManager() {
		return dom;
	}

	void Awake() {
		dom = this;
	}

	public void Show(int order, bool canCancel, string title, Sprite[] icons, string[] strings, Callback[] actions, Sprite[] buttons, Callback IntroCallback) {
		root.SetActive(true);
		SetOrder(order);
		SetData(canCancel, title, icons, strings, actions, buttons);
		StartCoroutine(ToggleRoutine(true, showDuration, IntroCallback));
	}

	void SetData(bool canCancel, string title, Sprite[] icons, string[] strings, Callback[] actions, Sprite[] buttons) {
		closeButton.SetActive(canCancel);
		if (title == "") {
			titleLong.gameObject.SetActive(false);
			titleShort.gameObject.SetActive(false);
		} else {
			bool isLong = title.Length > 5;

			titleLong.gameObject.SetActive(isLong);
			titleShort.gameObject.SetActive(!isLong);
			if (isLong)
				titleLong.text = title;
			else
				titleShort.text = title;
		}

		int targetLength = (icons == null) ? 0 : icons.Length;
		iconStringRoot.gameObject.SetActive(targetLength != 0);
		if (targetLength > 0) {
			while (iconStrings.Count > targetLength) {
				Destroy(iconStrings[iconStrings.Count - 1]);
				iconStrings.RemoveAt(iconStrings.Count - 1);
			}
			while (iconStrings.Count < targetLength) {
				iconStrings.Add(Instantiate(iconStringPrefab, iconStringRoot));
			}
			for (int i = 0; i < iconStrings.Count; ++i) {
				iconStrings[i].GetComponentInChildren<Image>().sprite = icons[i];
				iconStrings[i].GetComponentInChildren<Text>().text = strings[i];
			}
		}

		targetLength = (actions == null) ? 0 : actions.Length;
		buttonRoot.gameObject.SetActive(targetLength != 0);
		while (uiButtons.Count > targetLength) {
			if (uiButtons[uiButtons.Count -1] != null)
				uiButtons[uiButtons.Count -1].Destroy();
			uiButtons.RemoveAt(uiButtons.Count - 1);
		}
		while (uiButtons.Count < targetLength) {
			uiButtons.Add(Instantiate(buttonPrefab.gameObject, buttonRoot).GetComponent<UIButton>());
		}
		for (int i = 0; i < uiButtons.Count; ++i) {
			uiButtons[i].SetSprite(buttons[i]);
			int j = i;
			uiButtons[i].ClearOneShots();
			uiButtons[i].OneshotPress(()=> {CloseAction(actions[j]); });
		}
	}

	public void Close() {
		CloseAction(null);
	}

	public void CloseAction(Callback cb) {
		StartCoroutine(ToggleRoutine(false, showDuration, () => {root.SetActive(false); if (cb != null) cb(); }));
	}

	IEnumerator ToggleRoutine(bool on, float duration, Callback Done) {
		if (routineActive)
			interrupt = true;
		while (routineActive) {
			yield return null;
		}
		routineActive = true;
		interrupt = false;
		float timer = 0;
		Color startColor = (on) ? new Color(0, 0, 0, 0) : curtain.color;
		Color targetColor = (!on) ? new Color(0, 0, 0, 0) : curtain.color;
		InputManager.GetManager().SendingInputs = false;
		Vector3 startSize = (on) ? Vector3.zero : overlayRoot.localScale;
		Vector3 targetSize = (!on) ? Vector3.zero : overlayRoot.localScale;
		while (timer < duration && !DebugMaster.Instance.skipTransitions && !interrupt) {
			timer += Time.deltaTime;
			overlayRoot.localScale = Vector3.Lerp(startSize, targetSize, timer / duration);
			curtain.color = Color.Lerp(startColor, targetColor, timer / duration);
			yield return null;
		}
		InputManager.GetManager().SendingInputs = true;
		ViewManager.GetManager().SendCurrentViewInputs(!on);
		if (Done != null)
			Done();
		if (!on) {
			overlayRoot.localScale = startSize;
			curtain.color = startColor;
		} else {
			overlayRoot.localScale = targetSize;
			curtain.color = targetColor;
		}
		interrupt = false;
		routineActive = false; 
	}

	public UIButton GetPointedButton() {
		if (uiButtons == null || uiButtons.Count == 0)
			return null;
		return uiButtons[uiButtons.Count - 1];
	}

	public UIButton[] GetAllButtons() {

        UIButton[] buttons = new UIButton[uiButtons.Count + 1];
        for(int i = 0; i < uiButtons.Count; ++i)
            buttons[i] = uiButtons[i];
        buttons[buttons.Length - 1] = closeUIButton;
        return buttons;
	}
}
