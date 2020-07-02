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
	protected Vector3 childStart;
	protected bool moving;

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
		childStart = transform.GetChild(0).localPosition;
	}

	protected IEnumerator ScaleAndVibrate(float duration, float startScale, float targetScale, float delay = 0) {
		if (delay > 0)
			yield return new WaitForSeconds(delay);
		if (duration <= 0)
			yield break;
		float a = 0;
		Vector3 startPos = transform.GetChild(0).localPosition;
		Vector3 baseScale = transform.localScale;
		float vibrationAmount = 0.1f;
		while (a < 1) {
			a += Time.deltaTime / duration;
			transform.GetChild(0).localPosition = startPos + new Vector3(vibrationAmount * Random.Range(-1f, 1f), vibrationAmount * Random.Range(-1f, 1f));
			transform.localScale = Vector3.Lerp(baseScale * startScale, baseScale * targetScale, a * a);
			yield return null;
		}
		transform.GetChild(0).localPosition = Vector3.zero;
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
		transform.GetChild(0).localPosition = childStart;
	}

	public virtual void Pop() {
		alreadyPopped = true;
	}
}
