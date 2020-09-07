using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PairCard : Interactable {



	[SerializeField] SpriteRenderer spriteRenderer;

	Sprite frontSprite;
	Sprite backSprite;

	Callback Click;

	float speed = 0;
	float flipDuration = 0.2f;
	float delay = 0;

	bool targetReached = true;
	Vector3 targetPosition;
	Callback TargetReached;

	public bool Open {
		get; protected set;
	}

	public bool Rotating {
		get; protected set;
	}

	public void SetClick(Callback Click) {
		this.Click = Click;
	}

	public void SetSprites(Sprite front, Sprite back) {
		frontSprite = front;
		backSprite = back;
		UpdateSprite();
	}

	void Awake() {
		if (spriteRenderer == null)
			spriteRenderer = GetComponentInChildren<SpriteRenderer>();
	}

	void Update() {
		if (!targetReached && speed > 0) {
			if (delay > 0)
				delay -= Time.deltaTime;
			else {
				if (Vector3.SqrMagnitude(targetPosition - transform.position) < speed * speed * Time.deltaTime * Time.deltaTime) {
					transform.position = targetPosition;
					targetReached = true;
					TargetReached?.Invoke();
				} else
					transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
			}
		}
	}

	void UpdateSprite() {
		if (Open)
			spriteRenderer.sprite = frontSprite;
		else
			spriteRenderer.sprite = backSprite;
	}

	public override bool Interact(InteractType type) {
		if (base.Interact(type)) {
			if (Open || Rotating)
				return false;
			Click?.Invoke();
			return true;
		}
		return false;
	}

	public void Rotate(bool open, Callback Done) {
		if (open == Open)
			Done?.Invoke();
		else
			StartCoroutine(RotationRoutine(open, Done));
	}

	IEnumerator RotationRoutine(bool open, Callback Done) {
		Rotating = true;
		Quaternion face = Quaternion.Euler(0, 0, 0);
		Quaternion side = Quaternion.Euler(0, 90, 0);
		float a = 0;
		while (a < 1) {
			if (flipDuration > 0)
				a += Time.deltaTime / flipDuration;
			else
				a = 1;
			transform.rotation = Quaternion.Lerp(face, side, a);
			yield return null;
		}
		a = 0;
		Open = open;
		UpdateSprite();
		while (a < 1) {
			if (flipDuration > 0)
				a += Time.deltaTime / flipDuration;
			else
				a = 1;
			transform.rotation = Quaternion.Lerp(side, face, a);
			yield return null;
		}
		Rotating = false;
		Done?.Invoke();
	}

	public void Move(Vector3 target, float speed, float delay, Callback Done) {
		targetReached = false;
		TargetReached = Done;
		this.speed = speed;
		targetPosition = target;
		this.delay = delay;
	}

	protected override bool ValidInteracting(InteractType type) {
		return type == InteractType.Press;
	}
}
