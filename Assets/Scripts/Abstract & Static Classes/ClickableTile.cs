using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ClickableTile : Tile {

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
		StartCoroutine(ScaleAndVibrate(currentChild, duration, 1f, 0.25f));
	}

	public override void GrowVisual(float duration, float endSize = 1f) {
		StartCoroutine(ScaleAndVibrate(currentChild, duration, 0f, endSize));
	}

	public override void ShrinkVisual(float duration) {
		StartCoroutine(ScaleAndVibrate(currentChild, duration, 1f, 0f));
	}

	public override void ClickVisual(float duration) {
		StartCoroutine(ScaleAndVibrate(currentChild, duration * 3 / 4f, 1f, 1.5f));
		StartCoroutine(ScaleAndVibrate(currentChild, duration / 4f, 1f, 2 / 3f, duration * 3 / 4f));
	}

	public override void MoveToVisual(Vector3 position, float duration) {
		moving = true;
		StartCoroutine(MoveTowards(position, duration));
	}
}
