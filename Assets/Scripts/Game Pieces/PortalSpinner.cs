using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalSpinner : MonoBehaviour {

	[SerializeField] float speed = 0;
	[SerializeField] float[] ratios = new float[5] { 0.6f, 0.75f, 0.9f, 1.1f, 1.2f };
	float speedMultiplier = 1f;
	int direction;

	void OnEnable() {
		direction = Random.Range(0, 2) * 2 - 1;
		speedMultiplier = Random.Range(0.85f, 1.15f);
		for (int i = 0; i < transform.childCount; ++i) {
			transform.GetChild(i).rotation = Quaternion.Euler(0, 0, Random.Range(0, 360f));
		}
	}

	void Update() {
		for (int i = 0; i < transform.childCount; ++i) {
			transform.GetChild(i).rotation *= Quaternion.Euler(0, 0, direction * speed * speedMultiplier * ratios[i] * Time.deltaTime);
		}
	}
}
