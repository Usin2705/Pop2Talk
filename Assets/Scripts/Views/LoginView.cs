using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Components;

public class LoginView : View {

	[SerializeField] View characterSelectView = null;
	[SerializeField] View parentView = null;

	[SerializeField] View registerView = null;
	
	[SerializeField] View shipHubView = null;
	[Space]
	[SerializeField] InputField usernameField = null;
	[SerializeField] InputField passwordField = null;
	[SerializeField] UIButton playButton = null;
	[SerializeField] GameObject parentHolder = null;
	[SerializeField] UIButton parentButton = null;
	[SerializeField] GameObject waitingStore = null;
	[SerializeField] Toggle rememberToggle = null;

	[Space]
	[SerializeField] ConnectionStatus connectionStatus = null;
	[SerializeField] Text errorText = null;
	[Space]
	[SerializeField] string privacyPolicyUrl = "https://www.pop2talk.com/privacy-policy";


	string rememberKey = "remember";
	string usernameKey = "username";
	string passwordKey = "password";

	bool usedDeepLink;
	bool tryingToConnect;
	string delayOpenDeepLink;

	void Awake() {
		if (!string.IsNullOrEmpty(Application.absoluteURL)) {
			DeepLinkOpen(Application.absoluteURL);
		}

		Application.deepLinkActivated += DeepLinkDelayedOpen;
	}

	protected override void Initialize() {
		base.Initialize();

		//urchaseMaster.Instance.BeginInitialization();
		// Remove the store warning text
		waitingStore.SetActive(false);
		
		SoundEffectManager.GetManager().PlayMusic();
		playButton.SubscribePress(GameOnline);
		parentButton.SubscribePress(GotoParent);
		//StartCoroutine(StoreInitializationWait());
	}

	public override void Activate() {
		base.Activate();
		errorText.gameObject.SetActive(false);
		if (!usedDeepLink && EncryptedPlayerPrefs.GetInt(rememberKey, 0) == 1) {
			rememberToggle.isOn = true;
			usernameField.text = EncryptedPlayerPrefs.GetString(usernameKey);
			passwordField.text = EncryptedPlayerPrefs.GetString(passwordKey);
		}
	}

	void Update() {
		if (usernameField.isFocused || passwordField.isFocused) {
			errorText.gameObject.SetActive(false);
		}

		// if (PurchaseMaster.Instance.Initialized) {
		// 	if (waitingStore.activeSelf) {
		// 		waitingStore.SetActive(false);
		// 		parentHolder.SetActive(true);
		// 	}
		// }

		if (!string.IsNullOrEmpty(delayOpenDeepLink)) {
			DeepLinkOpen(delayOpenDeepLink);
		}
	}

	void GameOnline() {
		if (usernameField.text != "" && passwordField.text != "") {
			if (IsValidEmail(usernameField.text)) {
				// if (!PurchaseMaster.Instance.Subscribed) {
				// 	ShowError("error_parent");
				// 	return;
				// }
			}
			Online(ConnectedOnline);
		} else {
			ShowError("error_name");
		}
	}


	bool IsValidEmail(string email) {
		try {
			var addr = new System.Net.Mail.MailAddress(email);
			return addr.Address == email;
		}
		catch {
			return false;
		}
	}

	void CheckCharacter() {
		if (CharacterManager.GetManager().CurrentCharacter == null)
			GotoCharacterSelect();
		else
			GotoShipHub();
	}

	void OpenPrivacyPolicy() {
		OpenBrowser(privacyPolicyUrl);
	}

	void OpenBrowser(string url) {
		Application.OpenURL(url);
	}

	void Online(Callback Connected) {
		NetworkManager.GetManager().SetPlayer(usernameField.text);
		StartCoroutine(NetworkManager.GetManager().Login(usernameField.text, passwordField.text, errorText, ConnectWait(Connected), Connected));

	}

	IEnumerator ConnectWait(Callback Connected) {
		tryingToConnect = true;
		NetworkManager.GetManager().ServerWait(true);
		while (!NetworkManager.GetManager().Connected)
			yield return null;
		NetworkManager.GetManager().ServerWait(false);
		tryingToConnect = false;
		Connected();
	}

	IEnumerator StoreInitializationWait() {
		yield return new WaitForSeconds(2);
		if (!PurchaseMaster.Instance.Initialized) {
			NetworkManager.GetManager().ServerWait(true);
			while (!PurchaseMaster.Instance.Initialized)
				yield return null;
			NetworkManager.GetManager().ServerWait(false);
		}
	}

	void ConnectedOnline() {
		CheckRemember();
		connectionStatus.SetIngameName(NetworkManager.GetManager().Player);
		connectionStatus.ShowName(true);
		connectionStatus.ToggleConnection(true, true);
		CheckCharacter();
	}

	void CheckRemember() {
		if (rememberToggle.isOn) {
			EncryptedPlayerPrefs.SetInt(rememberKey, 1);
			EncryptedPlayerPrefs.SetString(usernameKey, usernameField.text);
			EncryptedPlayerPrefs.SetString(passwordKey, passwordField.text);
		} else if (!usedDeepLink) {
			EncryptedPlayerPrefs.SetInt(rememberKey, 0);
			if (EncryptedPlayerPrefs.HasKey(usernameKey))
				EncryptedPlayerPrefs.DeleteKey(usernameKey);
			if (EncryptedPlayerPrefs.HasKey(passwordKey))
				EncryptedPlayerPrefs.DeleteKey(passwordKey);
		}
	}

	void GotoCharacterSelect() {
		ViewManager.GetManager().ShowView(characterSelectView);
	}

	void GotoShipHub() {
		ViewManager.GetManager().ShowView(shipHubView);
	}

	void GotoParent() {
		if (usernameField.text.ToLower() == "parent") {
			ViewManager.GetManager().ShowView(registerView);
			//ViewManager.GetManager().ShowView(parentView);
		}
	}

	void ShowError(string key) {
		//errorText.gameObject.SetActive(true);
		//errorText.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = key;
	}
	
	void DeepLinkDelayedOpen(string url) {//Updating text from non-main thread doesn't update the visuals correctly
		delayOpenDeepLink = url;
	}

	void DeepLinkOpen(string url) {
		delayOpenDeepLink = null;
		StartCoroutine(DeepLinkRoutine(url));
	}

	IEnumerator DeepLinkRoutine(string url) {
		usedDeepLink = true;
		while (!initialized || tryingToConnect || (NetworkManager.GetManager() != null && NetworkManager.GetManager().Connected))
			yield return null;
		string[] loginData = url.Split('?');
		if (loginData.Length >= 2)
			usernameField.text = loginData[1];
		if (loginData.Length >= 3)
			passwordField.text = loginData[2];
		rememberToggle.isOn = false;
	}

	public override void Back() {
		base.Back();
		Application.Quit();
	}

	public override UIButton GetPointedButton() {
		return null;
	}

	public override UIButton[] GetAllButtons() {
		return null;
	}
}