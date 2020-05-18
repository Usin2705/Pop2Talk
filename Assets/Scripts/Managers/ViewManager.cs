using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewManager : MonoBehaviour {

	static ViewManager vm;

	[SerializeField] View firstView;
	[SerializeField] float openDelay;

	View currentView;

	Coroutine showViewRoutine;
	Queue<View> viewsToSwitch = new Queue<View>();

	public View CurrentView { get { return currentView; } }

	public static ViewManager GetManager() {
		return vm;
	}

	void Awake() {
		vm = this;
	}

	void Start() {
		firstView.Activate();
		currentView = firstView;
	}

	public void ShowView(View targetView, bool skipTransitions = false) {
		if (showViewRoutine == null)
			showViewRoutine = StartCoroutine(ShowViewRoutine(targetView, !skipTransitions && !DebugSettings.Instance.skipTransitions));
		else
			viewsToSwitch.Enqueue(targetView);
	}

	IEnumerator ShowViewRoutine(View targetView, bool showTransitions) {
		bool waiting = false;
		InputManager.GetManager().SendingInputs = false;

		if (showTransitions) {
			if (currentView != null && currentView.DoExitFluff) {
				waiting = true;
				currentView.ExitFluff(() => { waiting = false; });
				while (waiting)
					yield return null;
			}

			waiting = true;
			DoorCurtainManager.GetManager().CloseDoors(() => { waiting = false; });
			while (waiting)
				yield return null;
		}

		if (currentView != null) {
			currentView.Deactivate();
		}
		CharacterManager.GetManager().HideCharacter();
		ShipManager.GetManager().HideShip();
		targetView.Activate();
		currentView = targetView;
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

	public void SendCurrentViewInputs(bool send) {
		currentView.GetComponent<UnityEngine.EventSystems.BaseRaycaster>().enabled = send;
	}
}
