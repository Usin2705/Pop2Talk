﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideTile : ClickableTile {

	[SerializeField] SpriteRenderer tileRenderer;
	[SerializeField] SpriteRenderer hiderRenderer;

	bool hiding;

	public bool Hiding {
		get {
			return hiding;
		}
		set {
			hiding = value;
			tileRenderer.gameObject.SetActive(!hiding);
		}
	}

	public virtual void SetMatchType(MatchType type) {
		tileRenderer.sprite = GridManager.GetManager().GetMatchSprite(type);
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
		tileRenderer.transform.localScale = scale;
		hiderRenderer.transform.localScale = scale * 1.1f;
	}
}
