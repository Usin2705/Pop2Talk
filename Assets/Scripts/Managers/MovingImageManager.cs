using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovingImageManager : Overlay { 

	static MovingImageManager sm;
	[SerializeField] float pauseWobbleAmount;
	[SerializeField] float shipSpeed;
	[Space]
	[SerializeField] Image mover;
	[SerializeField] Image curtain;
	bool wobbling = false;

	public float BaseSize {
		get {
			return 100;
		}
	}

	public static MovingImageManager GetManager() {
		return sm;
	}

	public void Awake() {
		sm = this;
	}

	public void SetMoverSprite(Sprite sprite) {
		mover.sprite = sprite;
	}

	public void ShowMovingImage(int order, bool curtain, Vector3[] positions, float[] pauses, Vector3[] sizes, float[] speedMultipliers, AudioInstance[] sounds, Callback Done) {
		SetOrder(order);
		StartCoroutine(MovingImageRoutine(curtain, positions, pauses, sizes, speedMultipliers, sounds, Done));
		if (curtain)
			StartCoroutine(CurtainRoutine(0.5f, 0.7f));
	}

	IEnumerator MovingImageRoutine(bool curtain, Vector3[] positions, float[] pauses, Vector3[] sizes, float[] speeds, AudioInstance[] sounds, Callback Done) {
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
		mover.rectTransform.position = positions[0];
		for(int i = 0; i < sizes.Length; ++i) {
			sizes[i] = MathUtility.TermDivision(sizes[i], mover.rectTransform.parent.localScale);
		}
		mover.rectTransform.localScale = sizes[0];
		mover.gameObject.SetActive(true);
		for (int i = 0; i < positions.Length - 1; ++i) {
			if (sounds != null && sounds[i] != null)
				AudioMaster.Instance.Play(this, sounds[i]);
			while (Vector3.Distance(mover.rectTransform.position, positions[i + 1]) > Mathf.Epsilon) {
				mover.rectTransform.localScale = Vector3.Lerp(sizes[i]/BaseSize, sizes[i + 1]/BaseSize, Vector3.Distance(mover.rectTransform.position, positions[i]) / Vector3.Distance(positions[i], positions[i + 1]));
				mover.rectTransform.position = Vector3.MoveTowards(mover.rectTransform.position, positions[i + 1], speeds[i] * Time.deltaTime);
				yield return null;
			}
		   timer = 0;
			while (timer < pauses[i]) {
				timer += Time.deltaTime;
				mover.rectTransform.position = positions[i + 1] + (Vector3)Random.insideUnitCircle * pauseWobbleAmount;
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
		Vector3 startPos = mover.rectTransform.position;
		while (wobbling) {
			mover.rectTransform.position = startPos + (Vector3)Random.insideUnitCircle * wobbleAmount;
			yield return null;
		}
		mover.rectTransform.position = startPos;
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


	public void HideMover() {
		mover.gameObject.SetActive(false);
	}
}