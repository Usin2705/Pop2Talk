using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeBars : MonoBehaviour {
	
	[SerializeField] float floor;
	[SerializeField] float peak;
	[SerializeField] Sprite normalSprite;
	[SerializeField] Sprite quizSprite;

	public void Awake() {
		ResetBars();
	}

	private void OnDisable() {
		ResetBars();
	}

	public void Visualize(float[] samples) {
		if (peak <= 0 || peak < floor)
			return;
		float max = 0;
		float compare;
		foreach (float f in samples) {
			compare = Mathf.Abs(f);
			if (compare > max)
				max = compare;
		}
		max = (max - floor) / peak * transform.childCount;
		for (int i = 0; i < transform.childCount; ++i) {
			transform.GetChild(i).gameObject.SetActive(max > i);
		}
	}

	public void ResetBars() {
		for (int i = 0; i < transform.childCount; ++i) {
			transform.GetChild(i).gameObject.SetActive(false);
		}
	}

	public void SetQuiz(bool quiz) {
		for (int i = 0; i < transform.childCount; ++i) {
			transform.GetChild(i).GetComponent<Image>().sprite = (quiz) ? quizSprite : normalSprite;
		}
	}
}
