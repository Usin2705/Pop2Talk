using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioBar : MonoBehaviour {

    [SerializeField] float timeToMax = 0.3f;
	[SerializeField] Color normal;
	[SerializeField] Color quiz;

	Image image;
	float currentValue;
    float targetValue;
    float minValue;
    float maxValue;


    private void Awake() {
		image = GetComponentInChildren<Image>();
		Scale();
    }

    public void Stretch(float value, float min, float max) {
        minValue = min;
        maxValue = max;
        targetValue = Mathf.Min(value, max-min);
    }

	public void SetLength(float value) {
		if (minValue < value)
			minValue = value;
		targetValue = value - minValue;
		currentValue = targetValue;
		Scale();
	}

	public void SetQuiz(bool on) {
		if (image == null)
			image = GetComponentInChildren<Image>();
		image.color = (on) ? quiz : normal;
	}

    void Update() {
        if (targetValue != currentValue) {
            currentValue = Mathf.MoveTowards(currentValue, targetValue, (maxValue-minValue) * Time.deltaTime/timeToMax);
			Scale();
        }
    }

	void Scale() {
		if (image == null)
			image = GetComponentInChildren<Image>();
		image.transform.localScale = new Vector3(transform.localScale.x, currentValue + minValue, transform.localScale.z);
	}
}
