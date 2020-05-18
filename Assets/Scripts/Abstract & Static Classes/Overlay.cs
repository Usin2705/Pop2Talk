using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public abstract class Overlay : MonoBehaviour {

	protected Canvas canvas;

	public void SetOrder(int order) {
		if (canvas == null)
			canvas = GetComponent<Canvas>();
		canvas.sortingOrder = order+1;
	}

	public int GetOrder() {
		if (canvas == null)
			canvas = GetComponent<Canvas>();
		return canvas.sortingOrder;
	}
}
