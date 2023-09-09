using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.UI;
using System.Reflection;
using socket.io;
using UnityEngine.Localization.Components;



public class NetworkManager : MonoBehaviour {

	static NetworkManager netWorkManager;
	
	string url = Secret.URL;

	string loginUrl = Secret.LOGIN_URL;	
	string updateCharacterUrl = "api/game/update/character";	

	string asrURL = Secret.ASR_URL;

	// we skip this part for now
	//string coinUpdate = "api/game/update/coins";
	
	string uploadAudio = "api/game/upload/audio";
	string updateHighscore = "api/game/update/highscore";
	string getWordList = "api/game/create/wordlist";
	
	string updateLevelUrl = "api/game/update/level";

	string unlockCosmetic = "api/game/unlock/cosmetic";
	string equipCosmetic = "api/game/equip/cosmetic";

	string liveSocketUrl = "/recognizer";

	//string devAccount = "devel"; No longer used dev one
	//string devSocketUrl = ""; 

	Socket socket;
	
	string sessionId;

	UserData user;
	bool connected = false;
	bool serverWaitPromptActive;
	float secondsSinceStartup;
	float timeoutDuration = 4f;
	public float TimeoutDuration { get { return timeoutDuration; } }
	public bool Connected { get { return connected; } }
	string player = "Player not set";
	public string Player {
		get {
			return player;
		}

		private set {
			player = value;
		}
	}

	void Awake() {
		if (netWorkManager != null) {
			Debug.LogError("Multiple NetWorkManagers");
			Destroy(gameObject);
			return;
		}
		netWorkManager = this;
		sessionId = GetUniqueID();
		secondsSinceStartup = 0f;
	}

	public static NetworkManager GetManager() {
		return netWorkManager;
	}

	void OnDestroy() {

	}

	public void ServerWait(bool wait) {
		if (wait && !serverWaitPromptActive) {
			serverWaitPromptActive = true;
			DataOverlayManager.GetManager().Show(99, false, "server_wait", null, null, null, null, null);
		} else if (serverWaitPromptActive) {
			DataOverlayManager.GetManager().Close();
			serverWaitPromptActive = false;
		}
	}

	public void SetPlayer(string player) {
		Player = player;
	}

	public void ControlledExit() {
		if (socket != null && connected) {
			PlayerEvent pe = new PlayerEvent();
			pe.player = Player;
			socket.EmitJson("controlled_exit", JsonUtility.ToJson(pe));
			connected = false;
		}
	}

	public void LevelStarted(string level, bool passed, bool medal) {
		if (socket != null && connected) {
			LevelEvent le = new LevelEvent();
			le.player = Player;
			le.level = level;
			le.passed = passed;
			le.medal = medal;
			socket.EmitJson("start_level", JsonUtility.ToJson(le));
		}
	}

	public void LevelCompleted() {
		StartCoroutine(UpdateLevelRoutine());
	}

	IEnumerator UpdateLevelRoutine() {
		WWWForm form = new WWWForm();
		form.AddField("id", user.id);
		UnityWebRequest www = UnityWebRequest.Post(url + updateLevelUrl, form);
		yield return www.SendWebRequest();
		Debug.Log(www.downloadHandler.text);
	}

	// public void SamplePlayed(string level, string word, bool fromPop) {
	// 	AnalyticsEvent ae = new AnalyticsEvent {
	// 		eventname = "sample_played",
	// 		level = level,
	// 		sessionid = sessionId,
	// 		word = word,
	// 		fromPop = fromPop,
	// 	};
	// 	StartCoroutine(SendLoggableEvent(ae));
	// }

	public void SendMicrophone(string microphone, string word, AudioClip clip, IntCallback ScoreReceived, string challengetype, int retryAmount, bool feedback) {		
		StartCoroutine(UploadMicrophone(microphone, word, clip, ScoreReceived, challengetype, retryAmount, feedback));
	}

	IEnumerator UploadMicrophone(string microphone, string word, AudioClip clip, IntCallback ScoreReceived, string challengetype, int retryAmount, bool feedback) {
	    // IMultipartFormSection & MultipartFormFileSection  could be another solution,
		// but apparent it also require raw byte data to upload
		byte[] wavBuffer = SavWav.GetWav(clip, out uint length, trim:true);

		WWWForm form = new WWWForm();		
		form.AddField("user_id", user.id);
		form.AddField("session_id", user.session_id);
		form.AddField("timestamp", (DateTime.Now.Ticks/10000000).ToString());
		form.AddField("word", word);
		
		// If user gives consent, add audio data
		if (user.consent) {
			form.AddBinaryData("file", wavBuffer, fileName:"speech_sample", mimeType: "audio/wav");
		}

		UnityWebRequest www = UnityWebRequest.Post(url + uploadAudio, form);

		yield return www.SendWebRequest();

		// for (int i = 0; i < 20; ++i) {			
		// 	if (feedback) StartCoroutine(UploadAudioToASRServer(wavBuffer, word, ScoreReceived));

		// 	float randomDelay = UnityEngine.Random.Range(0.1f, 0.5f);
        // 	yield return new WaitForSeconds(randomDelay);
		// }		
		
		if (feedback) StartCoroutine(UploadAudioToASRServer(wavBuffer, word, ScoreReceived));
	}

	IEnumerator UploadAudioToASRServer(byte[] wavBuffer, string word, IntCallback ScoreReceived) {

		WWWForm form = new WWWForm();
		form.AddBinaryData("file", wavBuffer, fileName:"speech_sample", mimeType: "audio/wav");
		form.AddField("word", word);		
		form.AddField("project_id", user.project_id);			
		
		// Choose ASR URL based on project ID
		// if ((user.project_id == 5) || (user.project_id == 6)) {
		// 	asrURL = Secret.ASR_URL;
		// } else {
		// 	asrURL = Secret.ASR_URL;
		// }

		Debug.Log("ASR URL: " + asrURL);		
		
		UnityWebRequest www = UnityWebRequest.Post(asrURL, form);
		www.timeout = Const.TIME_OUT_SECS;

		// Save the current time
    	float startTime = Time.realtimeSinceStartup;

		yield return www.SendWebRequest();	

		// Calculate the elapsed time
    	float elapsedTime = Time.realtimeSinceStartup - startTime;
    	Debug.Log("Time taken: " + elapsedTime + " seconds");

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError) {
			Debug.Log(www.error);
			Debug.Log("Response Code: " + www.responseCode);
    		Debug.Log("Response Headers: " + www.GetResponseHeaders());
			throw new System.Exception(www.downloadHandler.text ?? www.error);
    throw new System.Exception(www.downloadHandler.text ?? www.error);
		} else {
			Debug.Log("Form upload complete!");

			if (www.downloadHandler.text == "invalid credentials") {
				Debug.Log("invalid credentials");
				yield break;
			}

			if (www.downloadHandler.text == "this account uses auth0") {
				Debug.Log("this account uses auth0");
				yield break;
			}
        }

		var json = SimpleJSON.JSON.Parse(www.downloadHandler.text);

		// Since we are connected, we can register the network status
		connected = true;
		
		// Trigger the score received callback
		ScoreReceived(json["stars"].AsInt);		
	}

	/// <summary>
	/// Logs in to the game server using the given username and password.
	/// </summary>
	/// <param name="username">The username of the player.</param>
	/// <param name="password">The password of the player.</param>
	/// <param name="errorText">A UI Text component to display error messages.</param>
	/// <param name="CoroutineCallback">The coroutine callback.</param>
	/// <param name="Connected">The callback method to be executed after a successful connection.</param>
	/// <returns>An IEnumerator for Unity's Coroutine system.</returns>
	public IEnumerator Login(string username, string password, Text errorText, IEnumerator CoroutineCallback, Callback Connected) {

		// // Check server account and set URLs accordingly
		// if (!Secret.SERVER_1_ACCOUNT.Contains(username)) {
		// 	url = Secret.URL2;
		// 	loginUrl = Secret.LOGIN_URL2;
		// } else {
		// 	url = Secret.URL;
		// 	loginUrl = Secret.LOGIN_URL;
		// }
		
		// Create a WWWForm and add fields for the username, password and local time
		WWWForm form = new WWWForm();
		form.AddField("username", username);
		form.AddField("password", password);
		form.AddField("timelocal", (DateTime.Now.Ticks/10000000).ToString());

		// Send POST request
		UnityWebRequest www = UnityWebRequest.Post(loginUrl, form);
		yield return www.SendWebRequest();		

		// Check if there was a connection error
		if (www.result == UnityWebRequest.Result.ConnectionError) 
		{
			// Log and handle connection error
			Debug.Log(www.error);
			errorText.text = www.error.Length <= 40 ? www.error : "Network connection not available.";			
			errorText.gameObject.SetActive(true);
			throw new System.Exception(www.error);
		} 	
		// Check if there was a protocol error	
		else if (www.result == UnityWebRequest.Result.ProtocolError) 
		{
			// Log and handle protocol error
			Debug.Log(www.error);
			if(www.error.Length <= 40)
				errorText.text = www.error;
			else if (www.responseCode == 404)
			{
				errorText.text = "The requested page could not be found.";
			}
			else
			{
			errorText.text = "A server error occurred.";
			}
			errorText.gameObject.SetActive(true);
			throw new System.Exception(www.error);
		} 		
		else 		
		{
			// If there are no errors, process the response
			Debug.Log("Form upload complete!");
			if (www.downloadHandler.text == "invalid credentials") {
				// Handle invalid credentials
				Debug.Log("invalid credentials");
				errorText.gameObject.SetActive(true);
				errorText.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = "error_invalid";				
				yield break;
			}

			if (www.downloadHandler.text == "this account uses auth0") {
				// Handle Auth0 accounts
				Debug.Log("this account uses auth0");
				errorText.gameObject.SetActive(true);
				errorText.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = "error_auth0";
				yield break;
			}
			
			// Since we are connected, we can register the network status
			connected = true;

			// Parse the response into a UserData object
			user = JsonUtility.FromJson<UserData>(www.downloadHandler.text);			
			Debug.Log("Login json: " + www.downloadHandler.text);
			SimpleJSON.JSONNode json = SimpleJSON.JSON.Parse(www.downloadHandler.text);

			// #####################################################################################
			// TODO Start to record the words and stars
			// WordMaster.Instance.ClearWords();

			// for (int i = 0; i < json["words"].Count; ++i) {
			// 	WordMaster.Instance.AddWord(json["words"][i]["word"].Value);
			// }

			// for (int i = 0; i < json["game_state"]["wordHighscores"].Count; ++i) {
			// 	WordMaster.Instance.SetStarAmount(json["game_state"]["wordHighscores"][i]["word"].Value, json["game_state"]["wordHighscores"][i]["maxstars"].AsInt);
			// }
			// #####################################################################################
			
			// There is 16 level --> largest module index = 15
			WordMaster.Instance.SetLargestModuleIndex(15);

			// Check if data is null by using IsNull
			if (!json["game_state"]["character"].IsNull)
				CharacterManager.GetManager().SetCharacter(json["game_state"]["character"].AsInt, false);

			CurrencyMaster.Instance.SetCoins(json["game_state"]["coins"].AsInt);
			
			// if (json["game_state"]["unlocked_cosmetics"].ToString() != "") {
			// 	for (int i = 0; i < json["game_state"]["unlocked_cosmetics"].Count; ++i) {
			// 		CosmeticManager.GetManager().UnlockCosmetic(json["game_state"]["unlocked_cosmetics"][i], false);
			// 	}
			// }

			// if (json["game_state"]["equipped_cosmetics"].ToString() != "") {
			// 	for (int i = 0; i < json["game_state"]["equipped_cosmetics"].Count; ++i) {
			// 		CosmeticManager.GetManager().EquipCosmetic(json["game_state"]["equipped_cosmetics"][i], false);
			// 	}
			// }

			// This get default cosmetics first
			// Without this line of code there's no default cosmetics for user
			CosmeticManager.GetManager().CheckDefaultCosmetics();

			// Trigger the Connected callback
			Connected();
		}
		
	}

	public void UpdateCoins(int coins) {
		StartCoroutine(CoinUpdateRoutine(coins));
	}

	IEnumerator CoinUpdateRoutine(int coins) {

		// TODO skip update internet
		yield return 0;

		// WWWForm form = new WWWForm();
		// form.AddField("coins", coins);
		// UnityWebRequest www = UnityWebRequest.Post(url + coinUpdate, form);
		// www.SetRequestHeader("Authorization", "Bearer " + user.access_token);
		// yield return www.SendWebRequest();
		
	}

	public void GetWordList(Callback Done) {
		StartCoroutine(WordListRoutine(Done));
	}

	IEnumerator WordListRoutine(Callback Done) {
	/*
	*	This return the word list from server
	*	The word list served as a series of exercise
	*	where user can play next or back?
	*   It's in the form of a json file
	*     - chosensWords [words]
	*	  - cardType [int card type] 0 is repeat 1 is memory
	*/	
		WWWForm form = new WWWForm();

		form.AddField("user_id", user.id);
		form.AddField("project_id", user.project_id);

		// TODO change this to GET request (need different code to send with form data)
		UnityWebRequest www = UnityWebRequest.Post(url + getWordList, form);
		yield return www.SendWebRequest();		
		
		Debug.Log(www.downloadHandler.text);

		// Since we are connected, we can register the network status
		connected = true;

        SimpleJSON.JSONNode json = SimpleJSON.JSON.Parse(www.downloadHandler.text);

		string[] words = new string[json["chosenWords"].Count];
		WordCardType[] types = new WordCardType[words.Length];

		Debug.Log("WordList json: " + www.downloadHandler.text);

		WordMaster.Instance.SetLargestModuleIndex(15);

		for (int i = 0; i < words.Length; ++i) {
			words[i] = json["chosenWords"][i];
			types[i] = (WordCardType) json["cardType"][i].AsInt;
			// Debug.Log("loop to find: " + words[i] + " Type: " + types[i]);
		}

		int level_index = json["level_index"].AsInt;
		int setting_index = json["setting_index"].AsInt;
		
		// This feedback settings is mainly for norwegian project
		int is_feedback_int = 1; // default value
		if (json.HasKey("is_feedback"))
		{
			is_feedback_int = json["is_feedback"].AsInt;
		}
		bool is_feedback = (is_feedback_int != 0); // convert to bool 0 is false, 1 is true		

		WordMaster.Instance.SetSamples(types, words, level_index, setting_index, is_feedback);
		Done?.Invoke();	
	}

	public void SendBestCardStar(string word, int score, int challenge) {
		
		StartCoroutine(BestCardStarRoutine(word, score, challenge));
	}

	IEnumerator BestCardStarRoutine(string word, int score, int challenge) {
	/*
	*
	*	Update session activity
	*
	*/
		WWWForm form = new WWWForm();
		form.AddField("session_id", user.session_id);
		form.AddField("last_activity", (DateTime.Now.Ticks/10000000).ToString());

		// TODO change this to GET request (need different code to send with form data)
		UnityWebRequest www = UnityWebRequest.Post(url + updateHighscore, form);		
		yield return www.SendWebRequest();		
		
		Debug.Log(www.downloadHandler.text);
	}

	public void UpdateCharacter(int character) {
		StartCoroutine(CharacterUpdateRoutine(character));
	}

	IEnumerator CharacterUpdateRoutine(int character) {
		WWWForm form = new WWWForm();
		form.AddField("id", user.id);
		form.AddField("character", character);
		UnityWebRequest www = UnityWebRequest.Post(url + updateCharacterUrl, form);
		yield return www.SendWebRequest();
		
		Debug.Log(www.downloadHandler.text);
	}
	
	public void UnlockCosmetic(string id) {
		//Debug.Log("Trying to unlock: " + id);
		//StartCoroutine(CosmeticUnlockRoutine(id));
	}

	IEnumerator CosmeticUnlockRoutine(string id) {
		WWWForm form = new WWWForm();
		form.AddField("id", id);
		UnityWebRequest www = UnityWebRequest.Post(url + unlockCosmetic, form);		
		yield return www.SendWebRequest();
		Debug.Log(www.downloadHandler.text);
	}

	public void EquipCosmetic(string id, int index) {
		//Debug.Log("Trying to equip: " + id);
		//StartCoroutine(CosmeticEquipRoutine(id, index));
	}

	IEnumerator CosmeticEquipRoutine(string id, int index) {
		WWWForm form = new WWWForm();
		form.AddField("id", id);
		form.AddField("index", index);
		UnityWebRequest www = UnityWebRequest.Post(url + equipCosmetic, form);		
		yield return www.SendWebRequest();
		Debug.Log(www.downloadHandler.text);
	}
	
	public bool GetConsent() {
		if (user != null)
			return user.consent;
		return false;
	}

	public void SimpleEvent(string name) {


		AnalyticsEvent ae = new AnalyticsEvent {
			eventname = name,
			player = Player,
			sessionid = sessionId,
		};
		StartCoroutine(SendLoggableEvent(ae));
	}

	public void LevelAbortEvent(string name, string stageName, string stageType, float levelDuration, int cardsCompleted, int averageStars, int totalStarsCollected, int stonesCollected) {


		AnalyticsEvent ae = new AnalyticsEvent {
			eventname = name,
			avgStars = averageStars,
			totalStars = totalStarsCollected,
			completedCards = cardsCompleted,
			stones = stonesCollected,
			leveltime = levelDuration,
			level = stageName,
			levelType = stageType,
			player = Player,
			sessionid = sessionId,
		};
		// TODO WE skip this event for now
		//StartCoroutine(SendLoggableEvent(ae));
	}

	public void SwipeEvent(string name) {
		if (socket == null)
			return;

		AnalyticsEvent ae = new AnalyticsEvent {
			eventname = name,
			player = Player,
			sessionid = sessionId,
		};
		StartCoroutine(SendLoggableEvent(ae));
	}

	IEnumerator SendLoggableEvent(AnalyticsEvent ae) {

		System.Version version = Assembly.GetExecutingAssembly().GetName().Version;
		System.DateTime startDate = new System.DateTime(2000, 1, 1, 0, 0, 0);
		System.TimeSpan span = new System.TimeSpan(version.Build, 0, 0, version.Revision * 2);
		System.DateTime buildDate = startDate.Add(span);
		ae.builddate = buildDate;
		ae.view = ViewManager.GetManager().CurrentView.name;

		ae.platform = Application.platform.ToString();
		ae.device = SystemInfo.deviceName.ToString();
		ae.sessiontime = secondsSinceStartup;

		ae.serverurl = liveSocketUrl;

		//socket.EmitJson("loggable_event", JsonUtility.ToJson(ae));
		yield return null;

		if (ae.eventname!="sample_played")
			Debug.Log("Event " + ae.eventname + " sent");
		
	}

	string GetUniqueID() {
		string[] split = System.DateTime.Now.TimeOfDay.ToString().Split(new Char[] { ':', '.' });
		string id = "";
		for (int i = 0; i < split.Length; i++) {
			id += split[i];
		}
		return id;
	}

}