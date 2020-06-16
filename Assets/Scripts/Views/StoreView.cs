using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreView : View {

	[SerializeField] View shipHubView;

	[SerializeField] UIButton backButton;
	

	protected override void Initialize() {
		base.Initialize();
		backButton.SubscribePress(Back);
	}

	void Back() {
		ViewManager.GetManager().ShowView(shipHubView);
	}

	public override UIButton GetPointedButton() {
		return null;
	}

	public override UIButton[] GetAllButtons() {
		return null;
	}
}
