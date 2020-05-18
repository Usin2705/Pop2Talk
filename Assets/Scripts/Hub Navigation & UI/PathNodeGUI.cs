using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathNodeGUI : MonoBehaviour {

	[SerializeField] Sprite lockedSprite;
	
    Sprite unlockedSprite;
    Image image;

    public void SetLocked(bool locked) {
        if (unlockedSprite == null) {
            image = GetComponentInChildren<Image>();
            unlockedSprite = image.sprite;
        }
        image.sprite = (locked) ? lockedSprite : unlockedSprite;
    }

    public void Disable() {
        if (unlockedSprite == null) {
            image = GetComponentInChildren<Image>();
            unlockedSprite = image.sprite;
        }
        image.gameObject.SetActive(false);
    }
}
