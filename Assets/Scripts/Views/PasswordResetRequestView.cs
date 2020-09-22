using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PasswordResetRequestView : View {

	[SerializeField] View loginView;
	[SerializeField] View passwordResetCompletionView;
	[SerializeField] GameObject gameHandler;
	[Space]
	[SerializeField] InputField emailField;
	[SerializeField] UIButton requestButton;
	[SerializeField] UIButton backButton;
	[SerializeField] Text errorText;
	[Space]
	[SerializeField] string serverUrl;
	[SerializeField] string requestPath;

	string requestStatus;

	protected override void Initialize() {
		base.Initialize();

		backButton.SubscribePress(Back);
		requestButton.SubscribePress(RequestReset);
	}

	public override void Activate() {
		base.Activate();
		errorText.gameObject.SetActive(false);
	}

	public override void Back() {
		base.Back();
		doExitFluff = false;
		ViewManager.GetManager().ShowView(loginView);
	}

	void RequestReset() {
		StartCoroutine("SendRequest");
		gameHandler.GetComponent<GameHandler>().emailHolder = emailField.text;
	}

	IEnumerator SendRequest() {
		WWWForm form = new WWWForm();
		form.AddField("email", emailField.text);
		UnityWebRequest www = UnityWebRequest.Post(serverUrl + requestPath, form);
		InputManager.GetManager().SendingInputs = false;
		errorText.gameObject.SetActive(false);
		yield return www.SendWebRequest();
		InputManager.GetManager().SendingInputs = true;
		if (www.isNetworkError || www.isHttpError) {
			//errorText.text = www.error;
			errorText.gameObject.SetActive(true);
			emailField.text = "";
		} else {
			Debug.Log(www.downloadHandler.text);
			requestStatus = www.downloadHandler.text;
			ViewManager.GetManager().ShowView(passwordResetCompletionView);
		}
	}

	public override UIButton[] GetAllButtons() {
		return null;
	}

	public override UIButton GetPointedButton() {
		return null;
	}
}
