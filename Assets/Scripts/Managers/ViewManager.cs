using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewManager : MonoBehaviour {

	static ViewManager vm;

	[SerializeField] View firstView = null;
	[SerializeField] float openDelay = 0;

	Coroutine showViewRoutine;
	Queue<View> viewsToSwitch = new Queue<View>();

	public View CurrentView { get; private set; }

	public static ViewManager GetManager() {
		return vm;
	}

	void Awake() {
		vm = this;
	}

	void Start() {
		firstView.Activate();
		CurrentView = firstView;
	}

	public void ShowView(View targetView, bool skipTransitions = false) {
		if (showViewRoutine == null)
			showViewRoutine = StartCoroutine(ShowViewRoutine(targetView, !skipTransitions && !DebugMaster.Instance.skipTransitions));
		else
			viewsToSwitch.Enqueue(targetView);
	}

	IEnumerator ShowViewRoutine(View targetView, bool showTransitions) {
		bool waiting = false;
		InputManager.GetManager().SendingInputs = false;

		if (showTransitions) {
			if (CurrentView != null && CurrentView.DoExitFluff) {
				waiting = true;
				CurrentView.ExitFluff(() => { waiting = false; });
				while (waiting)
					yield return null;
			}

			waiting = true;
			DoorCurtainManager.GetManager().CloseDoors(() => { waiting = false; });
			while (waiting)
				yield return null;
		}

		if (CurrentView != null) {
			CurrentView.Deactivate();
		}
		CharacterManager.GetManager().HideCharacter();
		ShipManager.GetManager().HideShip();
		targetView.Activate();
		CurrentView = targetView;
		yield return new WaitForSeconds(openDelay);

		if (showTransitions) {
			waiting = true;
			DoorCurtainManager.GetManager().OpenDoors(() => { waiting = false; });
			while (waiting) {
				yield return null;
			}

			waiting = true;
			targetView.EnterFluff(() => { waiting = false; });
			while (waiting)
				yield return null;
		}

		showViewRoutine = null;
		if (viewsToSwitch.Count > 0)
			ShowView(viewsToSwitch.Dequeue());
		else
			InputManager.GetManager().SendingInputs = true;
	}

	public void BackPressed() {
		CurrentView.Back();
	}

	public void SendCurrentViewInputs(bool send) {
		CurrentView.GetComponent<UnityEngine.EventSystems.BaseRaycaster>().enabled = send;
	}
}
