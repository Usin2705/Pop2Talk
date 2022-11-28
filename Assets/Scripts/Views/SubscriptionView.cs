using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Localization.Components;

//public class SubscriptionView : View, IPurchaseListener {
    public class SubscriptionView : View{

// 	[SerializeField] View parentView = null;
// 	[Space]
// 	[SerializeField] UIButton backButton = null;
// 	[SerializeField] UIButton subscriptionButton = null;
// 	[SerializeField] Text errorText = null;
// 	[SerializeField] Text priceText = null;
// 	[SerializeField] Text subText = null;

// 	string googleEntry = "google_entry";
// 	string iosEntry = "ios_entry";

// 	int googleSize = 67;
// 	int iosSize = 67;

// 	string priceTextBase;

// 	protected override void Initialize() {
// 		base.Initialize();
// 		backButton.SubscribePress(Back);
// 		subscriptionButton.SubscribePress(()=> {
// 			errorText.gameObject.SetActive(false);
// 			NetworkManager.GetManager().ServerWait(true);
// 			PurchaseMaster.Instance.PurchaseSubscription();
// 		});
// 		priceTextBase = priceText.text;
// #if UNITY_ANDROID
// 		subText.fontSize = iosSize;
// 		subText.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = googleEntry;
// #endif
// #if UNITY_IOS
// 		subText.fontSize = iosSize;
// 		subText.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = iosEntry;
// #endif
// 	}

// 	public override void Activate() {
// 		base.Activate();
// 		PurchaseMaster.Instance.Listener = this;
// 		errorText.gameObject.SetActive(false);
// 		subscriptionButton.gameObject.SetActive(true);
// 		priceText.text = priceTextBase + PurchaseMaster.Instance.SubscriptionPrice + "/month";
// 	}

// 	public override void Back() {
// 		base.Back();
// 		PurchaseMaster.Instance.Listener = null;
// 		doExitFluff = false;
// 		ViewManager.GetManager().ShowView(parentView);
// 	}

	public override UIButton[] GetAllButtons() {
		return null;
	}

	public override UIButton GetPointedButton() {
		return null;
	}

// 	public void PurchaseSuccesful() {
// 		NetworkManager.GetManager().ServerWait(false);
// 		Back();
// 	}

// 	public void PurchaseFailed() {
// 		NetworkManager.GetManager().ServerWait(false);
// 		errorText.gameObject.SetActive(true);
// 		errorText.text = "There was a problem with the purchase";
// 		subscriptionButton.gameObject.SetActive(true);
// 	}
}
