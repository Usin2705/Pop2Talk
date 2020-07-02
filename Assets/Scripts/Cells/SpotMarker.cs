using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotMarker : MonoBehaviour, IPoolable {

    Transform visual;
    Vector3 startScale;

    float shiftDuration = 0.4f;

    Coroutine activeShift;

    void Awake () {
		visual = transform.GetChild(0);
        startScale = visual.localScale;
	}

	public void Grow(float delay = 0) {
        Shift(Vector3.zero, startScale, false, delay);
    }

    public void Shrink() {
        Shift(startScale, Vector3.zero, true, 0);
    }

    public void Shift(Vector3 start, Vector3 end, bool destroy, float delay) {
        if (activeShift != null) {
            start = visual.localScale;
            StopCoroutine(activeShift);
        }
        activeShift = StartCoroutine(ShiftRoutine(start, end, destroy, delay));
    }

    IEnumerator ShiftRoutine(Vector3 start, Vector3 end, bool destroy, float delay) {
        float a = 0;
		visual.localScale = start;
		if (delay > 0)
			yield return new WaitForSeconds(delay);
		while (a < 1) {
            a += Time.deltaTime / shiftDuration;
            visual.localScale = Vector3.Lerp(start,end,a);
            yield return null;
        }
        activeShift = null;
        if (destroy) {
            PoolMaster.Instance.Destroy(gameObject);
        }
    }

    public void OnReturnToPool() {
        visual.localScale = startScale;
    }
}
