using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.EventSystems.EventSystem))]
public class InputManager : MonoBehaviour {

    bool sendingInputs = true;
	UnityEngine.EventSystems.EventSystem eventSystem;

	bool waitingRelease;
	Vector2 pressPosition;
	float pressTime;

	List<Callback> tryInputCallbacks = new List<Callback>();

	public bool SendingInputs {
        get {
            return sendingInputs;
        }

        set {
            sendingInputs = value;
			eventSystem.enabled = value;
        }
    }

    static InputManager inputManager;

    void Awake() {
        if (inputManager != null) {
            Debug.LogError("Multiple Input Managers");
            Destroy(gameObject);
            return;
        }
		eventSystem = GetComponent<UnityEngine.EventSystems.EventSystem>();
        inputManager = this;
    }
    
    public static InputManager GetManager() {
        return inputManager;
    }

    void Update () {
		if (SendingInputs)
			HandleInputs();
		else
			waitingRelease = false;
	}

    void HandleInputs() {
        if (Input.touches.Length > 0) {
            InteractType type;
            for (int i = 0; i < Input.touches.Length; ++i) {
                if (Input.touches[i].phase == TouchPhase.Began)
                    type = InteractType.Press;
                else if (Input.touches[i].phase == TouchPhase.Ended || Input.touches[i].phase == TouchPhase.Canceled)
                    type = InteractType.Release;
                else
                    type = InteractType.Hold;

                TryInput(Input.touches[i].position, type);
            }
        }

        if (Input.GetMouseButton(0)) {
            if (Input.GetMouseButtonDown(0))
                TryInput(Input.mousePosition, InteractType.Press);
            else
                TryInput(Input.mousePosition, InteractType.Hold);
        } else if (Input.GetMouseButtonUp(0)) {
            TryInput(Input.mousePosition, InteractType.Release);
        }
    }

    void TryInput(Vector2 position, InteractType type) {
		CheckForSwipe(position, type);
		foreach (Callback Cb in tryInputCallbacks)
			Cb();
		RaycastHit hit;
		if (Physics.Raycast(Camera.main.ScreenPointToRay(position), out hit, 100, ConstantHolder.interactableLayer)) {
            Interactable inter = hit.collider.GetComponentInParent<Interactable>();
			if (inter != null) {
				inter.SetLastInteractPosition(hit.point);
				inter.Interact(type);
			} else
				Debug.Log(hit.collider.name + " isn't interactable.");
        }
    }

	public void SubscribeTryinput(Callback callback) {
		if (callback != null)
			tryInputCallbacks.Add(callback);
	}

	void CheckForSwipe(Vector2 position, InteractType type) {
		if (type == InteractType.Press && !waitingRelease) {
			waitingRelease = true;
			pressPosition = position;
			pressTime = Time.time;
		}

		if (type == InteractType.Release) {
			if (waitingRelease) {
				float swipeMinDistance = 50f;
				float swipeMinTime = 0.1f;

				if (Time.time - pressTime > swipeMinTime && Vector2.Distance(position, pressPosition) > swipeMinDistance) {
                    NetworkManager.GetManager().SwipeEvent("swipe_event");
				}
			}
			waitingRelease = false;
		}
	}
}
