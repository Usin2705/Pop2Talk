using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

public class PurchaseMaster : IStoreListener {

	IStoreController controller;
	IExtensionProvider extensions;
	static PurchaseMaster instance;

	public static PurchaseMaster Instance {
		get {
			if (instance == null) {
				instance = new PurchaseMaster();

			}
			return instance;
		}
	}

	string oneMonthSub = "sub1m";

	bool attemptingInitialization;

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
	}

	public void OnInitializeFailed(InitializationFailureReason error) {
		attemptingInitialization = false;
	}

	public void OnPurchaseFailed(Product i, PurchaseFailureReason p) {
	}

	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e) {
		return PurchaseProcessingResult.Complete;
	}


}
