using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Localization.Components;

public class PasswordResetCompletionView : View {

	[SerializeField] View parentView = null;
	[SerializeField] GameObject gameHandler = null;
	[Space]
	[SerializeField] InputField resetCodeField = null;
	[SerializeField] InputField passwordField = null;
	[SerializeField] InputField passwordConfirmationField = null;
	[SerializeField] UIButton resetButton = null;
	[SerializeField] UIButton backButton = null;
	[SerializeField] Text errorText = null;
	[Space]
	[SerializeField] string serverUrl = "";
	[SerializeField] string resetPath = "";

	string requestStatus;

	private string email;


	protected override void Initialize() {
		base.Initialize();
		email = gameHandler.GetComponent<GameHandler>().emailHolder;
		gameHandler.GetComponent<GameHandler>().emailHolder = null;
		backButton.SubscribePress(Back);
		resetButton.SubscribePress(Reset);
	}

	public override void Back() {
		base.Back();
		doExitFluff = false;
		ViewManager.GetManager().ShowView(parentView);
	}

	void Reset() {
		if (passwordField.text != passwordConfirmationField.text) {
			errorText.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = "error_match";
			errorText.gameObject.SetActive(true);
			return;
		}

		PasswordScore passwordStrength = PasswordMaster.CheckStrength(passwordField.text);

		if (passwordStrength < PasswordMaster.GetRequiredScore()) {
			errorText.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = "error_strength";
			Debug.Log(passwordStrength);
			errorText.gameObject.SetActive(true);
			return;
		}

		StartCoroutine("SendRequest");
	}

	IEnumerator SendRequest() {
		WWWForm form = new WWWForm();
		form.AddField("email", email);
		form.AddField("code", resetCodeField.text);
		form.AddField("password", passwordField.text);
		InputManager.GetManager().SendingInputs = false;
		errorText.gameObject.SetActive(false);
		UnityWebRequest www = UnityWebRequest.Post(serverUrl + resetPath, form);
		yield return www.SendWebRequest();
		InputManager.GetManager().SendingInputs = true;
		
		if ((www.result == UnityWebRequest.Result.ConnectionError) || (www.result == UnityWebRequest.Result.ProtocolError)) {
			errorText.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = "error_code";
			errorText.gameObject.SetActive(true);
		} else {
			Debug.Log(www.downloadHandler.text);
			requestStatus = www.downloadHandler.text;
			ViewManager.GetManager().ShowView(parentView);
		}
	}

	public override UIButton[] GetAllButtons() {
		return null;
	}

	public override UIButton GetPointedButton() {
		return null;
	}
}
