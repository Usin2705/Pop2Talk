using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RegistrationView : View {

	[SerializeField] View parentView = null;
	[Space]
	[SerializeField] InputField usernameField = null;
	[SerializeField] InputField passwordField = null;
	[SerializeField] InputField passwordConfirmField = null;
	[SerializeField] UIButton registerButton = null;
	[SerializeField] UIButton backButton = null;
	[SerializeField] Toggle tosToggle = null;
	[SerializeField] Toggle newsletterToggle = null;
	[SerializeField] Text tosText = null;
	[Space]
	[SerializeField] Text errorText = null;
	[Space]
<<<<<<< Updated upstream
	[SerializeField] string serverUrl = "https://(user_management_api_server)/api/game/";
=======
    [SerializeField] Text errorText;
    [Space]
	[SerializeField] string serverUrl = "http://84.253.229.86:52705/api/game/";
>>>>>>> Stashed changes
	[SerializeField] string userCheckPath = "checkUsername";
	[SerializeField] string registerPath = "register";
	[SerializeField] string requestTosUrl = "http://84.253.229.86:52705/readServiceConditions";

	TermsAndConditions TOS;

	bool emailIsAvailable;

	protected override void Initialize() {
		base.Initialize();

		backButton.SubscribePress(Back);
		registerButton.SubscribePress(RegisterAccount);
	}

	void Start() {
		StartCoroutine("RequestNewestTOS");
	}

	public override void Back() {
		base.Back();
		doExitFluff = false;
		ViewManager.GetManager().ShowView(parentView);
	}

	void RegisterAccount() {
		//TODO: Add registration functionality
		Debug.Log("Registering");
		StartCoroutine("Register");
		ViewManager.GetManager().ShowView(parentView);
	}

	public void ValidateForm() {
		Debug.Log("Validating form");
		if (ValidateFormContent()) {
			Debug.Log("True");
			registerButton.gameObject.SetActive(true);
			errorText.gameObject.SetActive(false);
		} else {
			Debug.Log("False");
			registerButton.gameObject.SetActive(false);
		}
	}

	bool ValidateFormContent() {
		if (!IsValidEmail(usernameField.text)) {
			errorText.text = "please enter a valid email address.";
			errorText.gameObject.SetActive(true);
			return false;
		}
		StartCoroutine(EmailIsAvailable(usernameField.text));
		Debug.Log(usernameField.text);
		if (!emailIsAvailable) {
			errorText.text = "An account with this email address already exists.";
			errorText.gameObject.SetActive(true);
			return false;
		}
		PasswordScore passwordStrength = PasswordMaster.CheckStrength(passwordField.text);

		if (passwordStrength < PasswordMaster.GetRequiredScore()) {
			errorText.text = "Password is not strong enough. Please ensure that it is longer than 4 characters.";
			Debug.Log(passwordStrength);
			errorText.gameObject.SetActive(true);
			return false;
		}

		if (passwordConfirmField.text != passwordField.text) {
			errorText.text = "The password fields do not match.";
			errorText.gameObject.SetActive(true);
			return false;
		}

		if (!tosToggle.isOn) {
			errorText.text = "Please fully read and accept the Terms of Service and Privacy Policy.";
			errorText.gameObject.SetActive(true);
			return false;
		}

		errorText.text = "Your password is " + passwordStrength + ".";
		errorText.gameObject.SetActive(true);
		return true;
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

<<<<<<< Updated upstream
	IEnumerator Register() {
=======
	public static PasswordScore CheckStrength(string password)
	{
		int score = 2;
		
		if (string.IsNullOrEmpty(password)||string.IsNullOrWhiteSpace(password))
			score = (int)PasswordScore.Blank;
		if (password.Length < 4)
			score = (int)PasswordScore.VeryWeak;
		if (password.Length >= 8)
			score++;
		if (password.Length >= 12)
			score++;
		if (Regex.IsMatch(password, @"[0-9]+(\.[0-9][0-9]?)?", RegexOptions.ECMAScript))
		{
			Debug.Log("Password has numbers");
			score++;
		}
		if (Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z]).+$", RegexOptions.ECMAScript))
		{
			Debug.Log("Password has upper- and lowercase");
			score++;
		}
		if (Regex.IsMatch(password, @"[!,@,#,$,%,^,&,*,?,_,~,-,£,(,)]", RegexOptions.ECMAScript))
		{
			Debug.Log("Password has special symbol");
			score++;
		}
		
		Debug.Log(score);
		return (PasswordScore)score;
	}

	IEnumerator Register()
	{
>>>>>>> Stashed changes
		WWWForm form = new WWWForm();
		form.AddField("email", usernameField.text);
		form.AddField("password", passwordField.text);
		form.AddField("consent", newsletterToggle.isOn.ToString());
		UnityWebRequest www = UnityWebRequest.Post(serverUrl + registerPath, form);
		yield return www.SendWebRequest();
		if (www.isNetworkError || www.isHttpError) {
			errorText.text = www.error;
			Debug.LogError(www.downloadHandler.text);
			errorText.gameObject.SetActive(true);
		} else {
			Debug.Log(www.downloadHandler.text);
		}
	}

	IEnumerator EmailIsAvailable(string email) {
		WWWForm form = new WWWForm();
		form.AddField("email", email);
		UnityWebRequest www = UnityWebRequest.Post(serverUrl + userCheckPath, form);
		yield return www.SendWebRequest();
		if (www.isNetworkError || www.isHttpError) {
			errorText.text = www.error;
			Debug.LogError("The chosen email address already has an account!");
			errorText.gameObject.SetActive(true);
		} else {
			Debug.Log(www.downloadHandler.text);
			if (www.downloadHandler.text == "available") {
				emailIsAvailable = true;
				yield return null;
			} else {
				emailIsAvailable = false;
				yield return null;
			}
		}
	}

	IEnumerator RequestNewestTOS() {
		UnityWebRequest www = UnityWebRequest.Get(requestTosUrl);
		yield return www.SendWebRequest();
		if (www.isNetworkError || www.isHttpError) {
			errorText.text = www.error;
			Debug.LogError("Error accessing the terms of service!");
			errorText.gameObject.SetActive(true);
		} else {
			TOS = JsonUtility.FromJson<TermsAndConditions>(www.downloadHandler.text);
			Debug.Log(www.downloadHandler.text);
			tosText.text = TOS.termsOfService + "\n" + TOS.privacyPolicy;
		}
	}



	public override UIButton GetPointedButton() {
		return null;
	}

	public override UIButton[] GetAllButtons() {
		return null;
	}
}