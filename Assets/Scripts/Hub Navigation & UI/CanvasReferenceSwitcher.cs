using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class CanvasReferenceSwitcher : MonoBehaviour {
    
    CanvasScaler cs;

    void Awake() {
        cs = GetComponent<CanvasScaler>();    
    }

    void Update () {
		float ratio = cs.referenceResolution.y/cs.referenceResolution.x;
        cs.matchWidthOrHeight = (Screen.height/Screen.width > ratio) ? 0 : 1;
	}
}
