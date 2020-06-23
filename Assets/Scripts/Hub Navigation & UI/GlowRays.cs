using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowRays : MonoBehaviour {
	[SerializeField] float speed;

	void Update() {
		transform.Rotate(Vector3.forward, speed * Time.deltaTime);
	}
}
