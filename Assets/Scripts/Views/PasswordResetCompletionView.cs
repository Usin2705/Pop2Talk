using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PasswordResetCompletionView : View {

	[SerializeField] View loginView;
	[SerializeField] View passwordResetRequestView;
	[SerializeField] GameObject gameHandler;
	[Space]
	[SerializeField] InputField resetCodeField;
	[SerializeField] InputField passwordField;
	[SerializeField] InputField passwordConfirmationField;
	[SerializeField] UIButton resetButton;
	[SerializeField] UIButton backButton;
	[SerializeField] Text errorText;
	[Space]
	[SerializeField] string serverUrl;
	[SerializeField] string resetPath;

	string requestStatus;

	private string email;


	protected override void Initialize()
	{
		base.Initialize();
		email = gameHandler.GetComponent<GameHandler>().emailHolder;
		gameHandler.GetComponent<GameHandler>().emailHolder = null;
		backButton.SubscribePress(Back);
		resetButton.SubscribePress(Reset);
	}

	void Back()
	{
		doExitFluff = false;
		ViewManager.GetManager().ShowView(loginView);
	}

	void Reset()
	{
		Debug.Log(email);
		StartCoroutine("SendRequest");
		ViewManager.GetManager().ShowView(loginView);
	}

	IEnumerator SendRequest()
	{
		WWWForm form = new WWWForm();
		form.AddField("email", email);
		form.AddField("code", resetCodeField.text);
		form.AddField("password", passwordField.text);
		UnityWebRequest www = UnityWebRequest.Post(serverUrl + resetPath, form);
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
