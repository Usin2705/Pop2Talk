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
	public bool Renewing { get; protected set; }


	public string SubscriptionPrice { get; protected set; }

	string oneMonthSub = "en_gb_monthly_3.99_and_vat";

	bool attemptingInitialization;
	IStoreController controller;
	IExtensionProvider extensions;

	public bool Initialized {
		get {
			return controller != null && extensions != null;
		}
	}

	public IPurchaseListener Listener { get; set; }

	public void BeginInitialization() {
		if (Initialized || attemptingInitialization)
			return;
		var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
		builder.AddProduct(oneMonthSub, ProductType.Subscription);

#if UNITY_IOS || UNITY_STANDALONE_OSX
		var appleConfig = builder.Configure<IAppleConfiguration>();
		try {
			var receiptData = System.Convert.FromBase64String(appleConfig.appReceipt);
			AppleReceipt receipt = new AppleReceiptParser().Parse(receiptData);

			foreach (AppleInAppPurchaseReceipt productReceipt in receipt.inAppPurchaseReceipts) {
				if (productReceipt.productID == oneMonthSub) {
					if (productReceipt.subscriptionExpirationDate.CompareTo(System.DateTime.UtcNow) > 0) {
						Subscribed = true;
					}
				}
			}
		}
		catch (System.FormatException e) {
			//Receipt isn't in base64, most likely because of the fake-receipt in the editor
		}
#endif

		attemptingInitialization = true;
		UnityPurchasing.Initialize(this, builder);
	}


	public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
		attemptingInitialization = false;
		this.controller = controller;
		this.extensions = extensions;

		foreach (Product p in controller.products.all) {
			if (p.definition.id == oneMonthSub) {
				SubscriptionPrice = p.metadata.localizedPriceString;
				CheckRenewal(p);
			}
		}

		CheckSubscriptions();
	}

	public void OnInitializeFailed(InitializationFailureReason error) {
		attemptingInitialization = false;
	}

	public void PurchaseSubscription() {
		controller.InitiatePurchase(oneMonthSub);
	}


	public void OnPurchaseFailed(Product i, PurchaseFailureReason p) {
		Listener?.PurchaseFailed();
	}


	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e) {
		CheckSubscription(e.purchasedProduct);
		Listener?.PurchaseSuccesful();
		return PurchaseProcessingResult.Complete;
	}

	void CheckSubscriptions() {
		foreach (Product p in controller.products.all) {
			/*DebugMaster.Instance.DebugText("Id: " + p.definition.id + 
				"\nAvailable to purchase: " + p.availableToPurchase +
				"\nPrice " + p.metadata.localizedPriceString +
				"\nHas receipt: " + p.hasReceipt);*/
			CheckSubscription(p);
		}
	}

	void CheckSubscription(Product p) {
		if (!p.hasReceipt)
			return;

		if (p.definition.id == oneMonthSub) {
			Subscribed = true;
			CheckRenewal(p);
			//DebugMaster.Instance.DebugText("Subscribed!");
		}
	}

	void CheckRenewal(Product p) {
		if (Subscribed)
			Renewing = true;
		DebugMaster.Instance.DebugText("Checking Renewal!");
#if SUBSCRIPTION_MANAGER
		SubscriptionManager s = new SubscriptionManager(p, null);
		DebugMaster.Instance.DebugText("In subscription manager!");
		Renewing = s.getSubscriptionInfo().isCancelled() != Result.True;
		DebugMaster.Instance.DebugText(s.getSubscriptionInfo().isCancelled().ToString());
		if (!Renewing) {
			Renewing = s.getSubscriptionInfo().isAutoRenewing() != Result.False;
		}
		DebugMaster.Instance.DebugText(s.getSubscriptionInfo().isAutoRenewing().ToString());
#endif
	}
}