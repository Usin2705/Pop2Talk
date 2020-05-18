using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconManager : MonoBehaviour {

	public Sprite starIcon;
	public Sprite clickIcon;
	public Sprite moonstoneIcon;
	public Sprite medalIcon;
	public Sprite wordIcon;
	public Sprite arrowIcon;
	public Sprite replayIcon;
	public Sprite lockIcon;

	static IconManager im;

	public static IconManager GetManager() {
		return im;
	}

	void Awake() {
		im = this;
	}
}
