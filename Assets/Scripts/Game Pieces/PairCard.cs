using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PairCard : Interactable {

	Sprite frontSprite;
	Sprite backSprite;

	Callback Click;

	float speed = 0;

	public bool Open {
		get {
			return true;
		}
	}

	public bool Rotating {
		get {
			return true;
		}
	}

	void Start() {

	}

	public void SetSprites(Sprite front, Sprite back) {
		frontSprite = front;
		backSprite = back;
		UpdateSprite();
	}

	void UpdateSprite() {

	}

	public override bool Interact(InteractType type) {
		if (base.Interact(type)) {
			Click?.Invoke();
			return true;
		}
		return false;
	}

	public void Rotate(bool open, Callback Done) {

	}

	public void Move(Vector3 target) {
		Move(target, speed);
	}

	public void Move(Vector3 target, float speed) {

	}

	protected override bool ValidInteracting(InteractType type) {
		return type == InteractType.Press;
	}
}
