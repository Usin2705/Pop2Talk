using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishStar : MonoBehaviour {

	[SerializeField] Transform anchor;
	public float ratio = 1;
	public float angle;

	float range;

	void Start() {
		SetAnchor(anchor);
	}

	public void SetAnchor(Transform anchor) {
		this.anchor = anchor;
		range = Vector3.Distance(anchor.position, transform.position);
	}

	void Update() {
		transform.position = anchor.position + Quaternion.Euler(0, 0, -angle) *  Vector3.up * range *ratio;

	}
}
