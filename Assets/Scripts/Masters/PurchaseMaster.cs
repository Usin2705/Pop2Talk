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
					Subscribed = true;
					if (productReceipt.cancellationDate != null && productReceipt.cancellationDate.CompareTo(System.DateTime.Now) > 0)
						Renewing = true;
				}
			}
		}
		catch (System.FormatException e) {
			//Receipt isn't base64, such as in editor
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
			if (p.definition.id == oneMonthSub)
				SubscriptionPrice = p.metadata.localizedPriceString;
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
		if (Subscribed || !p.hasReceipt)
			return;

		if (p.definition.id == oneMonthSub) {
			Subscribed = true;
			Renewing = true;
#if SUBSCRIPTION_MANAGER
			SubscriptionManager s = new SubscriptionManager(p, null);
			Renewing = s.getSubscriptionInfo().isCancelled() != Result.True;
#endif
			//DebugMaster.Instance.DebugText("Subscribed!");
		}
	}


}