using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorCurtainManager : MonoBehaviour {

	static DoorCurtainManager dcm;

	[SerializeField] float openDuration;
	[SerializeField] float openPause;
	[SerializeField] float closeDuration;
	[SerializeField] float closePause;
	[Space]
	[SerializeField] RectTransform leftCurtain;
	[SerializeField] RectTransform rightCurtain;
	[SerializeField] RectTransform leftStart;
	[SerializeField] RectTransform rightStart;

	float openRatio = 0;
	Coroutine openRoutine;

	public static DoorCurtainManager GetManager() {
		return dcm;
	}

	void Awake() {
		dcm = this;
	}

	public void CloseDoors(Callback Done) {
		CloseDoors(Done, closeDuration, closePause);
	}

	public void CloseDoors(Callback Done, float duration, float pause) {
		ToggleDoor(false, Done, duration, pause);
	}

	public void OpenDoors(Callback Done) {
		OpenDoors(Done, openDuration, openPause);
	}

	public void OpenDoors(Callback Done, float duration, float pause) {
		ToggleDoor(true, Done, duration, pause);
	}

	void ToggleDoor(bool open, Callback Done, float duration, float pause) {
		StartCoroutine(DoorRoutine(open, Done, duration, pause));
	}

	IEnumerator DoorRoutine(bool open, Callback Done, float duration, float pause) {
		float targetAmount = (open) ? 0 : 1;
		if (!open) {
			leftCurtain.gameObject.SetActive(true);
			rightCurtain.gameObject.SetActive(true);

		}

		if (SoundEffectManager.GetManager() != null)
			AudioMaster.Instance.Play(this, (open) ? SoundEffectManager.GetManager().GetOpenSound() : SoundEffectManager.GetManager().GetCloseSound());

		while (openRatio != targetAmount) {
			openRatio = Mathf.MoveTowards(openRatio, targetAmount, Time.deltaTime / duration);
			leftCurtain.localPosition = Vector3.Lerp(leftStart.localPosition, Vector3.zero, openRatio);
			rightCurtain.localPosition = Vector3.Lerp(rightStart.localPosition, Vector3.zero, openRatio);
			yield return null;
		}
		if (open) {
			leftCurtain.gameObject.SetActive(false);
			rightCurtain.gameObject.SetActive(false);
		}
		yield return new WaitForSeconds (pause);
		Done();
	}
}
