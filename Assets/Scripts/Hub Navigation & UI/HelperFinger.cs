using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelperFinger : Overlay {

	[SerializeField] float noInputTime;
	[SerializeField] RectTransform finger;
    [SerializeField] float speed = 1.5f;
    [SerializeField] float distance = 1;

    float timer;

	bool fingerActive;
	bool previousCanInput;

	UIButton[] subscribedButtons;
	View currentView;

	UIButton pointTarget;

	void Start () {
		InputManager.GetManager().SubscribeTryinput(ResetTimer);
	}
	
	void Update () {
		View current = ViewManager.GetManager().CurrentView;
		if (current != currentView) {
			currentView = current;
			ClearFinger();
		}

		if (!fingerActive) {
			if (InputManager.GetManager().SendingInputs) {
				if (!previousCanInput) {
					ResetTimer();
					previousCanInput = true;
				}
				timer -= Time.deltaTime;
				if (timer < 0)
					ShowFinger();
		} else {
				previousCanInput = false;
			}
		}
    }

	void ResetTimer() {
		timer = noInputTime;
	}

	void ShowFinger() {
		if (fingerActive)
			return;
		subscribedButtons = currentView.GetAllButtons();
        if (subscribedButtons != null)
        {
            foreach (UIButton uib in subscribedButtons)
            {
                uib.SubscribePress(ClearFinger);
            }
        }
		pointTarget = currentView.GetPointedButton();
		if (pointTarget == null || subscribedButtons == null || subscribedButtons.Length == 0)
			return;
        ToggleFinger(true, currentView.GetOrder());
        StartCoroutine(PointFingerRoutine(finger.transform.position, pointTarget.transform.position));
    }

	void ClearFinger() {
		if (!fingerActive)
			return;
		foreach(UIButton uib in subscribedButtons) {
			uib.UnsubscribePress(ClearFinger);
		}
        StopCoroutine("PointFingerRoutine");
        ToggleFinger(false);
    }

	void ToggleFinger(bool on, int order = 0) {
		SetOrder(order + 1);
		fingerActive = on;
		finger.gameObject.SetActive(on);
		finger.transform.position = Vector3.MoveTowards(pointTarget.transform.position, finger.transform.parent.position, pointTarget.GetComponent<RectTransform>().rect.width / 16);
		finger.transform.rotation = Quaternion.LookRotation(Vector3.forward, pointTarget.transform.position - finger.transform.position);
	}

    IEnumerator PointFingerRoutine(Vector3 startPos, Vector3 targetPos)
    {
        finger.transform.position = startPos;
        float lerp = 0;

        Vector3 start = startPos;
        Vector3 end = start + (targetPos - start) / distance;


            while (fingerActive)
        {

            while (lerp < 1)
            {
                if (finger.transform.position == end)
                    lerp = 1;
                else
                    lerp += Time.deltaTime;
                finger.transform.position = Vector3.Lerp(start, end, lerp * speed);
                yield return null;
            }

            lerp = 0;
            while (lerp < 1)
            {
                if (finger.transform.position == start)
                    lerp = 1;
                else
                    lerp += Time.deltaTime;
                finger.transform.position = Vector3.Lerp(end, start, lerp * speed);
                yield return null;
            }

            lerp = 0;
            yield return null;
        }
    }
}
