using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SubscriptionView : View, IPurchaseListener {

	[SerializeField] View loginView = null;
	[Space]
	[SerializeField] UIButton backButton = null;
	[SerializeField] UIButton subscriptionButton = null;
	[SerializeField] Text errorText = null;
	[SerializeField] Text priceText = null;
	[SerializeField] Text subText = null;

	string googleText = "A subscription is required to play Pop2Talk from this device. The subscription is monthly and charged automatically, and uses the standard Google Play Store terms and conditions and cancellation policies. On your first subscription, you gain a 3-day free trial before you are charged anything. You can cancel the subscription during this free trial period and won't be charged once it ends.";
	string iosText = "A subscription is required to play Pop2Talk from this device. The subscription is monthly and charged automatically, and uses the standard App Store terms and conditions and cancellation policies. On your first subscription, you gain a 3-day free trial before you are charged anything. You can cancel the subscription during this free trial period and won't be charged once it ends.";

	int googleSize = 67;
	int iosSize = 67;

	protected override void Initialize() {
		base.Initialize();
		backButton.SubscribePress(Back);
		subscriptionButton.SubscribePress(()=> {
			errorText.gameObject.SetActive(false);
			NetworkManager.GetManager().ServerWait(true);
			PurchaseMaster.Instance.PurchaseSubscription();
		});
#if UNITY_ANDROID
		subText.text = googleText;
		subText.fontSize = googleSize;
#endif
#if UNITY_IOS
		subText.text = iosText;
		subText.fontSize = iosSize;
#endif
	}

	public override void Activate() {
		base.Activate();
		PurchaseMaster.Instance.Listener = this;
		errorText.gameObject.SetActive(false);
		priceText.text = PurchaseMaster.Instance.SubscriptionPrice + "/month";
		subscriptionButton.gameObject.SetActive(true);
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
		NetworkManager.GetManager().ServerWait(false);
		Back();
	}

	public void PurchaseFailed() {
		NetworkManager.GetManager().ServerWait(false);
		errorText.gameObject.SetActive(true);
		errorText.text = "There was a problem with the purchase";
		subscriptionButton.gameObject.SetActive(true);
	}
}
