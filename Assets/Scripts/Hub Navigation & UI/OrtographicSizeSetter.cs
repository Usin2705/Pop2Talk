using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrtographicSizeSetter : MonoBehaviour {
    
    Camera cam;
    float resolution;
    float size;
    float lastHeight;
    float lastWidth;

	void Awake () {
		cam = GetComponent<Camera>();
        lastHeight = 1920;
        lastWidth = 1080;
        resolution = lastHeight / lastWidth;
        size = cam.orthographicSize;
	}
	
	void Update () {
        if (lastHeight != Screen.height || lastWidth != Screen.width) {
            if (Screen.height / Screen.width > resolution)
                cam.orthographicSize = size / resolution * Screen.height / Screen.width;
            else
                cam.orthographicSize = size;
            lastHeight = Screen.height;
            lastWidth = Screen.width;
        }
	}
}
