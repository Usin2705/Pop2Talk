using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MatchType { None, Red, Blue, Yellow, Green, Orange, Purple, Joker = -1 };

public class MatchableTile : ClickableTile {

	[SerializeField] SpriteRenderer stencilRenderer = null;
	[SerializeField] SpriteRenderer regularRenderer = null;

	public MatchType MyMatchType { get; protected set; }

	public virtual void SetMatchType(MatchType type) {
		MyMatchType = type;
		stencilRenderer.sprite = GridManager.GetManager().GetMatchSprite(type);
		regularRenderer.sprite = GridManager.GetManager().GetMatchSprite(type);
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

	public void SetStencil(bool stencil) {
		currentChild = (stencil) ? stencilRenderer.transform : regularRenderer.transform;
		stencilRenderer.gameObject.SetActive(stencil);
		regularRenderer.gameObject.SetActive(!stencil);
	}

	public override void SetScale(Vector3 scale) {
		stencilRenderer.transform.localScale = scale;
		regularRenderer.transform.localScale = scale;
	}
}
