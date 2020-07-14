using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickCatcher : Interactable {

	public ICatcherTarget CatcherTarget { get; set; }

	public override bool Interact(InteractType type) {
		bool interact = base.Interact(type);
		if (interact && CatcherTarget != null) {
			CatcherTarget.CatchClick(interactPosition);
		} 

		return interact;
	}

	protected override bool ValidInteracting(InteractType type) {
		return CanInteract && (type == InteractType.Press);
	}
}
