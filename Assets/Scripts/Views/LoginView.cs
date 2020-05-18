using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginView : View {

	[SerializeField] View characterSelectView;
	[SerializeField] View registrationView;
	[SerializeField] View passwordResetView;
	[SerializeField] View wordRushView;
	[Space]
	[SerializeField] InputField usernameField;
    [SerializeField] InputField passwordField;
    [SerializeField] UIButton offlineButton;
	[SerializeField] UIButton onlineButton;
	[SerializeField] UIButton wordRushButton;
	[SerializeField] UIButton registerButton;
	[SerializeField] UIButton resetPasswordButton;
	[SerializeField] UIButton privacyPolicyButton;

    [Space]
	[SerializeField] ConnectionStatus connectionStatus;
    [SerializeField] Text errorText;
    [Space]
    [SerializeField] string privacyPolicyUrl = "https://www.pop2talk.com/privacy-policy";




    bool canOnline;

	protected override void Initialize() {
		base.Initialize();

		SoundEffectManager.GetManager().PlayMusic();
		offlineButton.SubscribePress(Offline);
		onlineButton.SubscribePress(GameOnline);
		wordRushButton.SubscribePress(RushOnline);
        privacyPolicyButton.SubscribePress(OpenPrivacyPolicy);
		registerButton.SubscribePress(Register);
		resetPasswordButton.SubscribePress(ResetPassword);
	}

	void Update() {
		bool prevOnline = canOnline;
        if (usernameField.isFocused || passwordField.isFocused)
            errorText.gameObject.SetActive(false);
        if (usernameField.text != "" && passwordField.text != "")
            canOnline = true;
        else
        {
            canOnline = false;
            if(errorText != null)
                errorText.gameObject.SetActive(false);
        }

        if (prevOnline != canOnline) {
			onlineButton.gameObject.SetActive(canOnline);
			//wordRushButton.gameObject.SetActive(canOnline);
		}

	}

	void Register()
	{
		GotoRegistration();
	}

	void ResetPassword()
	{
		GotoPasswordReset();
	}

	void Offline() {
		connectionStatus.SetIngameName("OFFLINE");
		connectionStatus.ShowName(true);
		connectionStatus.ToggleConnection(true, false);
		if (usernameField != null && usernameField.text.Length > 0)
			NetworkManager.GetManager().SetPlayer(usernameField.text);
		GotoCharacterSelect();
	}

	void GameOnline() {
        if (usernameField.text != "" && passwordField.text != "")
        {
            Online(GotoCharacterSelect);
        }
        else
        {
            errorText.gameObject.SetActive(true);
            errorText.text = "Please enter username and password.";
        }
	}


	void RushOnline() {
		Online(GotoWordRush);
	}

    void OpenPrivacyPolicy()
    {
        OpenBrowser(privacyPolicyUrl);
    }

    void OpenBrowser(string url)
    {
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
		connectionStatus.SetIngameName(NetworkManager.GetManager().Player);
		connectionStatus.ShowName(true);
		connectionStatus.ToggleConnection(true, true);
		Connected();
	}

	void GotoCharacterSelect() {
		ViewManager.GetManager().ShowView(characterSelectView);
	}

	void GotoRegistration()
	{
		ViewManager.GetManager().ShowView(registrationView);
	}
	void GotoPasswordReset()
	{
		ViewManager.GetManager().ShowView(passwordResetView);
	}

	void GotoWordRush() {
		ViewManager.GetManager().ShowView(wordRushView);
	}

	public override UIButton GetPointedButton() {
        return null;
	}

	public override UIButton[] GetAllButtons() {
        return null;
    }
}