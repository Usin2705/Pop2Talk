using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MatchType { None, Red, Blue, Yellow, Green, Orange, Purple, Joker = -1 };

public class MatchableTile : Tile {

	[SerializeField] SpriteRenderer stencilRenderer;
	[SerializeField] SpriteRenderer regularRenderer;

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

	public override bool Interact(InteractType type) {
		if (!base.Interact(type) || moving)
			return false;

		if (Receiver == null || !Receiver.CanClick()) {
			return false;
		}

		if (type == InteractType.Press) {
			Receiver.ClickTile(this);
			return true;
		} else
			return false;
	}

	public override void PopVisual(float duration) {
		StartCoroutine(ScaleAndVibrate(duration, 1f, 0.25f));
	}

	public override void GrowVisual(float duration) {
		StartCoroutine(ScaleAndVibrate(duration, 0f, 1f));
	}

	public override void ClickVisual(float duration) {
		StartCoroutine(ScaleAndVibrate(duration * 3 / 4f, 1f, 1.5f));
		StartCoroutine(ScaleAndVibrate(duration / 4f, 1f, 2 / 3f, duration * 3 / 4f));
	}

	public override void MoveToVisual(Vector3 position, float duration) {
		moving = true;
		StartCoroutine(MoveTowards(position, duration));
	}

	public void SetStencil(bool stencil) {
		stencilRenderer.gameObject.SetActive(stencil);
		regularRenderer.gameObject.SetActive(!stencil);
	}
}
