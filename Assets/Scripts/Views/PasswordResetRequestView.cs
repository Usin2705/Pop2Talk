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

	protected override void Initialize()
	{
		base.Initialize();

		backButton.SubscribePress(Back);
		requestButton.SubscribePress(RequestReset);
	}

	void Back()
	{
		doExitFluff = false;
		ViewManager.GetManager().ShowView(loginView);
	}

	void RequestReset()
	{
		StartCoroutine("SendRequest");
		gameHandler.GetComponent<GameHandler>().emailHolder = emailField.text;
		ViewManager.GetManager().ShowView(passwordResetCompletionView);
	}

	IEnumerator SendRequest()
	{
		WWWForm form = new WWWForm();
		form.AddField("email", emailField.text);
		UnityWebRequest www = UnityWebRequest.Post(serverUrl + requestPath, form);
		yield return www.SendWebRequest();
		if (www.isNetworkError || www.isHttpError)
		{
			errorText.text = www.error;
			Debug.LogError(www.downloadHandler.text);
			errorText.gameObject.SetActive(true);
		}
		else
		{
			Debug.Log(www.downloadHandler.text);
			requestStatus = www.downloadHandler.text;
		}
	}

	public override UIButton[] GetAllButtons()
	{
		throw new System.NotImplementedException();
	}

	public override UIButton GetPointedButton()
	{
		throw new System.NotImplementedException();
	}
}
