using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotMarker : MonoBehaviour, IPoolable {

    Transform ring;
    Vector3 startScale;

    float shiftDuration = 0.4f;

    Coroutine activeShift;

    void Awake () {
		ring = transform.GetChild(0);
        startScale = ring.localScale;
	}
    
    public void Grow(Callback Done) {
        Shift(Vector3.zero, startScale, false);
    }

    public void Shrink() {
        Shift(startScale, Vector3.zero, true);
    }

    public void Shift(Vector3 start, Vector3 end, bool destroy) {
        if (activeShift != null) {
            start = ring.localScale;
            StopCoroutine(activeShift);
        }
        activeShift = StartCoroutine(ShiftRoutine(start, end, destroy));
    }

    IEnumerator ShiftRoutine(Vector3 start, Vector3 end, bool destroy) {
        float a = 0;
        while (a < 1) {
            a += Time.deltaTime / shiftDuration;
            ring.localScale = Vector3.Lerp(start,end,a);
            yield return null;
        }
        activeShift = null;
        if (destroy) {
            PoolMaster.Instance.Destroy(gameObject);
        }
    }

    public void OnReturnToPool() {
        ring.localScale = startScale;
    }
}
