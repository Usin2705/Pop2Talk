using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SubscriptionView : View, IPurchaseListener {

	[SerializeField] View loginView;
	[Space]
	[SerializeField] UIButton backButton;
	[SerializeField] UIButton subscriptionButton;
	[SerializeField] Text errorText;
	[SerializeField] Text priceText;
	

	protected override void Initialize() {
		base.Initialize();
		backButton.SubscribePress(Back);
		subscriptionButton.SubscribePress(PurchaseMaster.Instance.PurchaseSubscription);
	}

	public override void Activate() {
		base.Activate();
		PurchaseMaster.Instance.Listener = this;
		errorText.gameObject.SetActive(false);
		priceText.text = PurchaseMaster.Instance.SubscriptionPrice + "/month";
	}

	public override void Back() {
		base.Back();
		PurchaseMaster.Instance.Listener = null;
		doExitFluff = false;
		ViewManager.GetManager().ShowView(loginView);
	}

	public override UIButton[] GetAllButtons() {
		return null;
	}

	public override UIButton GetPointedButton() {
		return null;
	}

	public void PurchaseSuccesful() {
		Back();
	}

	public void PurchaseFailed() {
		errorText.gameObject.SetActive(true);
		errorText.text = "There was a problem with the purchase";
	}
}
