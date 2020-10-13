using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginView : View {

	[SerializeField] View characterSelectView = null;
	[SerializeField] View registrationView = null;
	[SerializeField] View subscriptionView = null;
	[SerializeField] View passwordResetView = null;
	[SerializeField] View shipHubView = null;
	[Space]
	[SerializeField] InputField usernameField = null;
	[SerializeField] InputField passwordField = null;
	[SerializeField] UIButton playButton = null;
	[SerializeField] GameObject registrationHolder = null;
	[SerializeField] UIButton registerButton = null;
	[SerializeField] GameObject resetHolder = null;
	[SerializeField] UIButton resetPasswordButton = null;
	[SerializeField] GameObject subscriptionHolder = null;
	[SerializeField] UIButton subscriptionButton = null;
	[SerializeField] GameObject waitingStore = null;
	[SerializeField] UIButton privacyPolicyButton = null;
	[SerializeField] Toggle rememberToggle = null;

	[Space]
	[SerializeField] ConnectionStatus connectionStatus = null;
	[SerializeField] Text errorText = null;
	[Space]
	[SerializeField] string privacyPolicyUrl = "https://www.pop2talk.com/privacy-policy";


	string rememberKey = "remember";
	string usernameKey = "username";
	string passwordKey = "password";

	protected override void Initialize() {
		base.Initialize();

		PurchaseMaster.Instance.BeginInitialization();
		SoundEffectManager.GetManager().PlayMusic();
		playButton.SubscribePress(GameOnline);
		privacyPolicyButton.SubscribePress(OpenPrivacyPolicy);
		registerButton.SubscribePress(Register);
		resetPasswordButton.SubscribePress(ResetPassword);
		subscriptionButton.SubscribePress(GotoSubscription);
		if (EncryptedPlayerPrefs.GetInt(rememberKey, 0) == 1) {
			rememberToggle.isOn = true;
			usernameField.text = EncryptedPlayerPrefs.GetString(usernameKey);
			passwordField.text = EncryptedPlayerPrefs.GetString(passwordKey);
		}
		StartCoroutine(StoreInitializationWait());
	}

	public override void Activate() {
		base.Activate();
		errorText.gameObject.SetActive(false);
		subscriptionHolder.SetActive(false);
		registrationHolder.SetActive(false);
		resetHolder.SetActive(false);
	}

	void Update() {
		if (usernameField.isFocused || passwordField.isFocused)
			errorText.gameObject.SetActive(false);

		if (PurchaseMaster.Instance.Initialized) {
			if (waitingStore.activeSelf)
				waitingStore.SetActive(false);

			if (usernameField.text == "" || IsValidEmail(usernameField.text)) {
				if (!PurchaseMaster.Instance.Renewing) {
					if (!subscriptionHolder.activeSelf)
						subscriptionHolder.SetActive(true);
					if (registrationHolder.activeSelf)
						registrationHolder.SetActive(false);
					if (resetHolder.activeSelf)
						resetHolder.SetActive(false);
				} else {
					if (subscriptionHolder.activeSelf)
						subscriptionHolder.SetActive(false);
					if (!registrationHolder.activeSelf)
						registrationHolder.SetActive(true);
					if (!resetHolder.activeSelf)
						resetHolder.SetActive(true);
				}
			} else {
				if (subscriptionHolder.activeSelf)
					subscriptionHolder.SetActive(false);
				if (registrationHolder.activeSelf)
					registrationHolder.SetActive(false);
				if (resetHolder.activeSelf)
					resetHolder.SetActive(false);
			}
		}

	}

	void Register() {
		GotoRegistration();
	}

	void ResetPassword() {
		GotoPasswordReset();
	}

	void GotoSubscription() {
		ViewManager.GetManager().ShowView(subscriptionView);
	}

	void GameOnline() {
		if (usernameField.text != "" && passwordField.text != "") {
			if (IsValidEmail(usernameField.text)) {
				if (!PurchaseMaster.Instance.Subscribed) {
					errorText.gameObject.SetActive(true);
					errorText.text = "This device doesn't have an active subscription.";
					return;
				}
			}
			Online(ConnectedOnline);
		} else {
			errorText.gameObject.SetActive(true);
			errorText.text = "Please enter your username and password.";
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
		NetworkManager.GetManager().ServerWait(true);
		while (!NetworkManager.GetManager().Connected)
			yield return null;
		NetworkManager.GetManager().ServerWait(false);
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
		} else {
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

	void GotoRegistration() {
		ViewManager.GetManager().ShowView(registrationView);
	}

	void GotoPasswordReset() {
		ViewManager.GetManager().ShowView(passwordResetView);

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