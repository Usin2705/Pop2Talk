using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MulticardView : View, IMinigame {

	[SerializeField] View shipHub;
	[SerializeField] GameObject cardPrefab;
	[SerializeField] Vector3 scale;
	[SerializeField] Vector3 center;
	[SerializeField] Sprite[] cardBacks;
	[Space]
	[SerializeField] UIButton backButton;
	

	protected override void Initialize() {
		base.Initialize();
		backButton.SubscribePress(GotoShipHub);
	}

	public override void Activate() {
		base.Activate();
	}

	public override void Deactivate() {
		base.Deactivate();
	}

	void GotoShipHub() {
		ViewManager.GetManager().ShowView(shipHub);
	}

	public override UIButton GetPointedButton() {
		return null;
	}

	public override UIButton[] GetAllButtons() {
		return null;
	}

	public Sprite GetIcon() {
		return cardBacks[0];
	}
}
