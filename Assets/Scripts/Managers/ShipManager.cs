using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipManager : Overlay { 

	static ShipManager sm;
	[SerializeField] float pauseWobbleAmount = 0;
	[Space]
	[SerializeField] RectTransform mover = null;
	[SerializeField] Image curtain = null;
	[SerializeField] Image shipTop = null;
	[SerializeField] Image shipMid = null;
	[SerializeField] Image shipBottom = null;
	bool wobbling = false;

	public float BaseSize {
		get {
			return 100;
		}
	}

	public static ShipManager GetManager() {
		return sm;
	}

	public void Awake() {
		sm = this;
	}

	public void SetShipSprites(Sprite top, Sprite mid, Sprite bottom) {
		shipTop.sprite = top;
		shipMid.sprite = mid;
		shipBottom.sprite = bottom;
	}

	public void ShowShipMovement(int order, bool curtain, Vector3[] positions, float[] pauses, Vector3[] sizes, float[] speedMultipliers, AudioInstance[] sounds, Callback Done) {
		SetOrder(order);
		StartCoroutine(MovingShipRoutine(curtain, positions, pauses, sizes, speedMultipliers, sounds, Done));
		if (curtain)
			StartCoroutine(CurtainRoutine(0.5f, 0.7f));
	}

	IEnumerator MovingShipRoutine(bool curtain, Vector3[] positions, float[] pauses, Vector3[] sizes, float[] speeds, AudioInstance[] sounds, Callback Done) {
		float timer;
		if (!(positions.Length == (pauses.Length+1) == (positions.Length == sizes.Length))) {
			Debug.LogError("Arrays in shipmotion not all same lengths");
			Done();
			yield break;
		}

		if (positions.Length == 0) {
			Debug.LogError("Array lengths are zero");
			Done();
			yield break;
		}
		mover.position = positions[0];
		for(int i = 0; i < sizes.Length; ++i) {
			sizes[i] = MathUtility.TermDivision(sizes[i], mover.parent.localScale);
		}
		mover.localScale = sizes[0];
		mover.gameObject.SetActive(true);
		for (int i = 0; i < positions.Length - 1; ++i) {
			if (sounds != null && sounds[i] != null)
				AudioMaster.Instance.Play(this, sounds[i]);
			while (Vector3.Distance(mover.position, positions[i + 1]) > Mathf.Epsilon) {
				mover.localScale = Vector3.Lerp(sizes[i]/BaseSize, sizes[i + 1]/BaseSize, Vector3.Distance(mover.position, positions[i]) / Vector3.Distance(positions[i], positions[i + 1]));
				mover.position = Vector3.MoveTowards(mover.position, positions[i + 1], speeds[i] * Time.deltaTime);
				yield return null;
			}
		   timer = 0;
			while (timer < pauses[i]) {
				timer += Time.deltaTime;
				mover.position = positions[i + 1] + (Vector3)Random.insideUnitCircle * pauseWobbleAmount;
				yield return null;
			}
		}
		if (curtain)
			StartCoroutine(CurtainRoutine(0.25f, 0f));
		Done();
	}

	public void StartWobble(float wobbleAmount) {
		wobbling = true;
		StartCoroutine(WobbleRoutine(wobbleAmount));
	}
	
	public void EndWobble() {
		wobbling = false;
	}

	IEnumerator WobbleRoutine(float wobbleAmount) {
		Vector3 startPos = mover.position;
		while (wobbling) {
			mover.position = startPos + (Vector3)Random.insideUnitCircle * wobbleAmount;
			yield return null;
		}
		mover.position = startPos;
	}

	IEnumerator CurtainRoutine(float showDuration, float curtainAlpha) {
		float lerp = 0;
		float start = curtain.color.a;
		if (start == curtainAlpha)
			yield break;

		curtain.gameObject.SetActive(true);

		while (lerp < 1) {
			if (showDuration <= 0)
				lerp = 1;
			else
				lerp += Time.deltaTime / showDuration;
			curtain.color = new Color(0, 0, 0, Mathf.Lerp(start, curtainAlpha, lerp));
			yield return null;
		}
		if (curtainAlpha == 0)
			curtain.gameObject.SetActive(false);
	}


	public void HideShip() {
		mover.gameObject.SetActive(false);
	}
}