using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginView : View {

	[SerializeField] View characterSelectView;
	[SerializeField] View registrationView;
	[SerializeField] View subscriptionView;
	[SerializeField] View passwordResetView;
	[SerializeField] View shipHubView;
	[Space]
	[SerializeField] InputField usernameField;
	[SerializeField] InputField passwordField;
	[SerializeField] UIButton playButton;
	[SerializeField] UIButton registerButton;
	[SerializeField] UIButton resetPasswordButton;
	[SerializeField] GameObject subscriptionHolder;
	[SerializeField] UIButton subscriptionButton;
	[SerializeField] UIButton privacyPolicyButton;
	[SerializeField] Toggle rememberToggle;

	[Space]
	[SerializeField] ConnectionStatus connectionStatus;
	[SerializeField] Text errorText;
	[Space]
	[SerializeField] string privacyPolicyUrl = "https://www.pop2talk.com/privacy-policy";


	string rememberKey = "remember";
	string usernameKey = "username";
	string passwordKey = "password";

	bool canOnline;

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
	}

	public override void Activate() {
		base.Activate();
		errorText.gameObject.SetActive(false);
		subscriptionHolder.SetActive(false);
	}

	void Update() {
		bool prevOnline = canOnline;
		if (usernameField.isFocused || passwordField.isFocused)
			errorText.gameObject.SetActive(false);
		if (usernameField.text != "" && passwordField.text != "" && PurchaseMaster.Instance.Initialized)
			canOnline = true;
		else {
			canOnline = false;
			if (errorText != null)
				errorText.gameObject.SetActive(false);
		}

		if (prevOnline != canOnline) {
			playButton.gameObject.SetActive(canOnline);
		}
		if (PurchaseMaster.Instance.Initialized) {
			if (!subscriptionHolder.activeSelf && !PurchaseMaster.Instance.Renewing) {
				subscriptionHolder.SetActive(true);
			}
		}

	}

	void Register() {
		DebugMaster.Instance.DebugText("Register!");
		GotoRegistration();
	}

	void ResetPassword() {
		DebugMaster.Instance.DebugText("Reset password!");
		GotoPasswordReset();
	}

	void GotoSubscription() {
		DebugMaster.Instance.DebugText("Subscribe!");
		ViewManager.GetManager().ShowView(subscriptionView);
	}

	void GameOnline() {
		DebugMaster.Instance.DebugText("Game Online!");
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