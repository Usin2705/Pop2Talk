using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public abstract class Tile : Interactable {

	/*public override bool Interact(InteractType type) {
        if (!base.Interact(type))
            return false;

        if (type == InteractType.Press)
            return true;
        else
            return false;
    }*/

	protected bool alreadyPopped;
	protected Vector3 childStartPos;
	protected Vector3 childStartScale;
	protected bool moving;
	protected Transform currentChild;

	protected float vibrationAmount = 0.1f;

	public virtual bool CanPop {
		get {
			return !alreadyPopped;
		}
	}

	public ITileClickReceiver Receiver {
		get; set;
	}


	public virtual void PopVisual(float duration) {

	}

	public virtual void GrowVisual(float duration) {

	}

	public virtual void ClickVisual(float duration) {

	}

	public virtual void MoveToVisual(Vector3 position, float duration) {

	}

	protected virtual void Awake() {
		currentChild = transform.GetChild(0);
		childStartPos = currentChild.localPosition;
		childStartScale = currentChild.localScale;
	}

	protected IEnumerator ScaleAndVibrate(float duration, float startScale, float targetScale, float delay = 0) {
		if (delay > 0)
			yield return new WaitForSeconds(delay);
		if (duration <= 0)
			yield break;
		float a = 0;
		Vector3 startPos = currentChild.localPosition;
		Vector3 baseScale = childStartScale;
		while (a < 1) {
			a += Time.deltaTime / duration;
			currentChild.localPosition = startPos + new Vector3(vibrationAmount * Random.Range(-1f, 1f), vibrationAmount * Random.Range(-1f, 1f));
			currentChild.localScale = Vector3.Lerp(baseScale * startScale, baseScale * targetScale, a * a);
			yield return null;
		}
		currentChild.localPosition = Vector3.zero;
	}

	protected IEnumerator MoveTowards(Vector3 targetPosition, float duration) {
		Vector3 startPosition = transform.position;
		moving = true;
		float a = 0;
		while (a < 1) {
			a += Time.deltaTime / duration;
			transform.position = Vector3.LerpUnclamped(startPosition, targetPosition, a) - Vector3.forward; // -Vector3 forward so that moving tile is on top
			yield return null;
		}
		transform.position = targetPosition;
		moving = false;
	}

	public virtual void Reset() {
		alreadyPopped = false;
		currentChild.localPosition = childStartPos;
	}

	public virtual void Pop() {
		alreadyPopped = true;
	}

	public virtual void SetScale(Vector3 scale) {
		currentChild.localScale = scale;
	}
}
