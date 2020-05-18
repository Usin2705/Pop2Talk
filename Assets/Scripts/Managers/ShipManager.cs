using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipManager : Overlay { 

	static ShipManager sm;
	[SerializeField] float pauseWobbleAmount;
	[SerializeField] float shipSpeed;
	[Space]
	[SerializeField] Image ship;

    private Vector3 velocity = Vector3.zero;

	public static ShipManager GetManager() {
		return sm;
	}

	public void Awake() {
		sm = this;
	}

	public void SetShip(Sprite shipSprite) {
		ship.sprite = shipSprite;
	}

	public void ShowShipMotion(int order, Vector3[] positions, float[] pauses, Vector3[] sizes, float[] speedMultipliers, Callback Done) {
		SetOrder(order);
		StartCoroutine(ShipMotionRoutine(positions, pauses, sizes, speedMultipliers, Done));
	}

	IEnumerator ShipMotionRoutine(Vector3[] positions, float[] pauses, Vector3[] sizes, float[] speedMultipliers, Callback Done) {
		float timer;
		if (!(positions.Length == pauses.Length == (positions.Length == sizes.Length))) {
			Debug.Log("Arrays in shipmotion not all same lengths");
			Done();
			yield break;
		}

		if (positions.Length == 0) {
			Debug.Log("Array lengths are zero");
			Done();
			yield break;
		}
		ship.rectTransform.position = positions[0];
		for(int i = 0; i < sizes.Length; ++i) {
			sizes[i] = MathUtility.TermDivision(sizes[i], ship.rectTransform.parent.localScale);
		}
		ship.rectTransform.localScale = sizes[0];
		ship.gameObject.SetActive(true);
		for (int i = 0; i < positions.Length - 1; ++i) {
			AudioMaster.Instance.Play(this, SoundEffectManager.GetManager().GetRocketZoomSound());
			while (Vector3.Distance(ship.rectTransform.position, positions[i + 1]) > 0.05f) {
				ship.rectTransform.localScale = Vector3.Lerp(sizes[i], sizes[i + 1], Vector3.Distance(ship.rectTransform.position, positions[i]) / Vector3.Distance(positions[i], positions[i + 1]));
                ship.rectTransform.position = Vector3.SmoothDamp(ship.rectTransform.position, positions[i + 1], ref velocity, 0.3f);
				yield return null;
			}
			timer = 0;
			while (timer < pauses[i]) {
				timer += Time.deltaTime;
				ship.rectTransform.position = positions[i + 1] + (Vector3)Random.insideUnitCircle * pauseWobbleAmount;
				yield return null;
			}
		}
		Done();
	}

	public void HideShip() {
		ship.gameObject.SetActive(false);
	}

}