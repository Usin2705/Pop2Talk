using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

public class PurchaseMaster : IStoreListener {

	static PurchaseMaster instance;

	public static PurchaseMaster Instance {
		get {
			if (instance == null) {
				instance = new PurchaseMaster();

			}
			return instance;
		}
	}

	public bool Subscribed { get; protected set; }

	string oneMonthSub = "en_gb_monthly_3.99_and_vat";

	bool attemptingInitialization;
	IStoreController controller;
	IExtensionProvider extensions;

	public bool Initialized {
		get {
			return controller != null && extensions != null;
		}
	}

	public void BeginInitialization() {
		if (Initialized || attemptingInitialization)
			return;
		var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
		builder.AddProduct(oneMonthSub, ProductType.Subscription);

		attemptingInitialization = true;
		UnityPurchasing.Initialize(this, builder);
	}

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
		attemptingInitialization = false;
		this.controller = controller;
		this.extensions = extensions;

		CheckGoogleSubscription();
	}

	public void OnInitializeFailed(InitializationFailureReason error) {
		attemptingInitialization = false;
	}

	public void OnPurchaseFailed(Product i, PurchaseFailureReason p) {
	}

	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e) {
		return PurchaseProcessingResult.Complete;
	}

	void CheckGoogleSubscription() {
#if UNITY_ANDROID
		foreach (Product p in controller.products.all) {
			GooglePurchaseData data = new GooglePurchaseData(p.receipt);

			if (p.hasReceipt && data.json.productId == oneMonthSub) {
				Subscribed = true;
				return;
			}
		}
#endif
	}

}

class GooglePurchaseData {

	public string inAppPurchaseData;
	public string inAppDataSignature;

	public GooglePurchaseJson json;

	[System.Serializable]
	private struct GooglePurchaseReceipt {
		public string Payload;
	}

	[System.Serializable]
	private struct GooglePurchasePayload {
		public string json;
		public string signature;
	}

	[System.Serializable]
	public struct GooglePurchaseJson {
		public string autoRenewing;
		public string orderId;
		public string packageName;
		public string productId;
		public string purchaseTime;
		public string purchaseState;
		public string developerPayload;
		public string purchaseToken;
	}

	public GooglePurchaseData(string receipt) {
		try {
			var purchaseReceipt = JsonUtility.FromJson<GooglePurchaseReceipt>(receipt);
			var purchasePayload = JsonUtility.FromJson<GooglePurchasePayload>(purchaseReceipt.Payload);
			var inAppJsonData = JsonUtility.FromJson<GooglePurchaseJson>(purchasePayload.json);

			inAppPurchaseData = purchasePayload.json;
			inAppDataSignature = purchasePayload.signature;
			json = inAppJsonData;
		}
		catch {
			Debug.Log("Could not parse receipt: " + receipt);
			inAppPurchaseData = "";
			inAppDataSignature = "";
		}
	}
}