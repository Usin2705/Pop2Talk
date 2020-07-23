using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideTile : ClickableTile {

	[SerializeField] SpriteRenderer tileRenderer;
	[SerializeField] SpriteRenderer hiderRenderer;
	[SerializeField] Sprite[] hideSprites;

	bool hiding;

	public bool Hiding {
		get {
			return hiding;
		}
		set {
			hiding = value;
			tileRenderer.gameObject.SetActive(hiding);
		}
	}

	public override void Reset() {
		base.Reset();
		vibrationAmount = 0;
		currentChild = hiderRenderer.transform;
		currentChild.localScale = Vector3.zero;
		float random = Random.Range(0.6f, 1f);
		hiderRenderer.color = new Color(random, random, random);
		Sprite s = hideSprites.GetRandom();
		foreach (SpriteRenderer rend in hiderRenderer.transform.parent.GetComponentsInChildren<SpriteRenderer>())
			rend.sprite = s;
	}

	public void HideHider() {
		currentChild.localScale = Vector3.zero;
	}

	public void FadeHide(float duration) {
		currentChild = transform;
		PopVisual(duration);
	}

	public void GrowReveal(float duration) {
		StartCoroutine(ScaleAndVibrate(tileRenderer.transform, duration, 0f, 1f));
		Debug.Log(childStartScale);
		StartCoroutine(ScaleAndVibrate(hiderRenderer.transform.parent.GetChild(1), duration, 0f, 1f));
	}


	public override void PopVisual(float duration) {
		StartCoroutine(ScaleAndVibrate(currentChild, duration, 1f, 0.01f));
	}

	public virtual void SetMatchType(MatchType type) {
		foreach(SpriteRenderer rend in tileRenderer.GetComponentsInChildren<SpriteRenderer>())
			rend.sprite = GridManager.GetManager().GetMatchSprite(type);
	}

	public void SetRandomMatchType(int maxTypes = -1, HashSet<MatchType> excludedTypes = null) {
		if (maxTypes < ConstantHolder.minimumTypes) {
			maxTypes = ConstantHolder.numberOfTypes;
		}
		maxTypes = Mathf.Clamp(maxTypes, ConstantHolder.minimumTypes, ConstantHolder.numberOfTypes);
		MatchType type;
		if (excludedTypes != null && maxTypes <= excludedTypes.Count) {
			Debug.LogError("Excluded matchtypes outnumber available types");
			return;
		}
		do {
			type = (MatchType)Random.Range(0, maxTypes) + 1;
		} while (type == MatchType.None || (excludedTypes != null && excludedTypes.Contains(type)));
		SetMatchType(type);
	}

	public override void SetScale(Vector3 scale) {
		transform.localScale = Vector3.one;
		tileRenderer.transform.localScale = scale;
		hiderRenderer.transform.localScale = scale;
		hiderRenderer.transform.parent.GetChild(1).localScale = scale;
	}
}
