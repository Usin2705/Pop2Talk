using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Localization.Components;

public class ParentView : View {

	[SerializeField] View loginView = null;
	[SerializeField] View subscriptionView = null;
	[SerializeField] View registerView = null;
	[SerializeField] View resetView = null;
	[Space]
	// No longer use ParentView and RestorationResult
	//[SerializeField] Text errorText = null;
	[SerializeField] UIButton backButton = null;
	// [SerializeField] UIButton restoreButton = null; No longer use ParentView
	[SerializeField] UIButton subscribeButton = null;
	[SerializeField] UIButton registerButton = null;
	[SerializeField] UIButton resetButton = null;
	[SerializeField] UIButton tosButton = null;
	[SerializeField] UIButton privacyButton = null;

	// No longer use ParentView and RestorationResult
	// bool canRestore = true; 

	protected override void Initialize() {
		base.Initialize();
		backButton.SubscribePress(Back);
		subscribeButton.SubscribePress(() => { ViewManager.GetManager().ShowView(subscriptionView); });
		registerButton.SubscribePress(() => { ViewManager.GetManager().ShowView(registerView); });
		resetButton.SubscribePress(() => { ViewManager.GetManager().ShowView(resetView); });
		tosButton.SubscribePress(() => { Application.OpenURL("https://www.pop2talk.com/terms-of-use"); });
		privacyButton.SubscribePress(() => { Application.OpenURL("https://www.pop2talk.com/privacy-policy"); });
		
		//restoreButton.SubscribePress(() => { PurchaseMaster.Instance.RestorePurchases(RestorationResult); });
	}

	public override void Activate() {
		base.Activate();
// We don't need purchase master for this version		
// 		if (PurchaseMaster.Instance.Renewing) {
// 			subscribeButton.gameObject.SetActive(false);
// 			restoreButton.gameObject.SetActive(false);
// 			registerButton.gameObject.SetActive(true);
// 			resetButton.gameObject.SetActive(true);
// 			errorText.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = "active_subscription";
// 		} else {
// 			subscribeButton.gameObject.SetActive(true);
// #if UNITY_IOS
// 			restoreButton.gameObject.SetActive(canRestore);
// #endif
// 			registerButton.gameObject.SetActive(false);
// 			resetButton.gameObject.SetActive(false);
// 			errorText.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = "no_subscription";
// 		}
	}

	public override void Back() {
		base.Back();
		doExitFluff = false;
		ViewManager.GetManager().ShowView(loginView);
	}

	public override UIButton[] GetAllButtons() {
		return null;
	}

	public override UIButton GetPointedButton() {
		return null;
	}

	// No longer use ParentView and RestorationResult
	// void RestorationResult(bool result) {
	// 	if (result) {
	// 		canRestore = false;
	// 		ViewManager.GetManager().ShowView(this);
	// 	} else {
	// 		errorText.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = "restore_failed";
	// 	}
	// }
}
