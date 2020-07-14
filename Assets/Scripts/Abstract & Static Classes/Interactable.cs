using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractType { None, Press, Hold, Release};

public abstract class Interactable : MonoBehaviour, IPoolable {

	protected Vector3 interactPosition;

	public bool CanInteract { get; protected set; } = true;

	public virtual void SetLastInteractPosition(Vector3 pos) {
		interactPosition = pos;
	}

    public virtual bool Interact(InteractType type) {
        if (!ValidInteracting(type))
            return false;

        return true;
    }

    public virtual void OnReturnToPool() {
    }

    protected virtual bool ValidInteracting(InteractType type) {
        return CanInteract && type != InteractType.None;
    }
}
