using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PasswordResetRequestView : View {

	[SerializeField] View parentView = null;
	[SerializeField] View passwordResetCompletionView = null;
	[SerializeField] GameObject gameHandler = null;
	[Space]
	[SerializeField] InputField emailField = null;
	[SerializeField] UIButton requestButton = null;
	[SerializeField] UIButton backButton = null;
	[SerializeField] Text errorText = null;
	[Space]
	[SerializeField] string serverUrl = "";
	[SerializeField] string requestPath = "";

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
		ViewManager.GetManager().ShowView(parentView);
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
