using System.Collections;
using System.Collections.Generic;
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

	string loginUrl = "api/game/login";	
	string updateCharacterUrl = "api/game/update/character";	

	string scoreUrl = "api/game/model/score";
	
	// we skip this part for now
	//string coinUpdate = "api/game/update/coins";
	
	string updateHighscore = "api/game/update/highscore";
	string getWordList = "api/game/create/wordlist";
	
	string updateLevelUrl = "api/game/update/level";

	string unlockCosmetic = "api/game/unlock/cosmetic";
	string equipCosmetic = "api/game/equip/cosmetic";

	string liveSocketUrl = "/recognizer";

	//string devAccount = "devel"; No longer used dev one
	//string devSocketUrl = ""; 

	Socket socket;
	bool waitingScore;

	string sessionId;

	private sbyte[] samplesSbyte;
	private short[] samplesShort;

	private byte[] voiceData;

	bool reconnecting = false;

	UserData user;

	int fs = 16000;
	float packetGap = 0.125f;
	float okGap = 1f;

	// Add data transfer defaults:
	// dataencoding can be
	//    'pcm'    -- No compression
	//    'mulaw'  -- Logarithmic compression of pcm values
	string dataencoding = "mulaw";
	//float mu = 255f;

	// datatype can be:
	//  'int16'  16 bit signed integer (short) 'h'
	//   'int8'   8 bit signed char    (sbyte) 'b'
	string datatype = "int8";

	float time;
	float connectAttemptTime;
	float checkOkTime;

	bool connected = false;
	bool serverWaitPromptActive;
	bool attemptingToConnect;

	float secondsSinceStartup;

	public float secondsSinceStart { get { return secondsSinceStartup; } }

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

	private void Update() {
		time = Time.time; // Time.time isn't threadsafe
		if (connected && attemptingToConnect) {
			attemptingToConnect = false;
			//socket.Off("i_am_ok");
		} else if (socket != null && !connected && time > checkOkTime) {
			checkOkTime = time + okGap;
			//socket.Emit("are_you_ok");
			/*if (!connecting)
                Reconnect();*/
		} else if (reconnecting) {
			//Reconnect();
		}

		secondsSinceStartup = Time.realtimeSinceStartup;
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


	public void SamplePlayed(string level, string word, bool fromPop) {
		AnalyticsEvent ae = new AnalyticsEvent {
			eventname = "sample_played",
			level = level,
			sessionid = sessionId,
			word = word,
			fromPop = fromPop,
		};
		StartCoroutine(SendLoggableEvent(ae));
	}

	public void SendMicrophone(string microphone, string word, AudioClip clip, float duration, IntCallback ScoreReceived, string challengetype, int retryAmount) {
		StartCoroutine(UploadMicrophone(microphone, word, clip, duration, ScoreReceived, challengetype, retryAmount));
	}

	IEnumerator UploadMicrophone(string microphone, string word, AudioClip clip, float duration, IntCallback ScoreReceived, string challengetype, int retryAmount) {
	    // IMultipartFormSection & MultipartFormFileSection  could be another solution,
		// but apparent it also require raw byte data to upload
		byte[] wavBuffer = SavWav.GetWav(clip, out uint length, trim:true);

		WWWForm form = new WWWForm();
        form.AddBinaryData("file", wavBuffer, fileName:"speech_sample", mimeType: "audio/wav");
        form.AddField("word", word);
		form.AddField("user_id", user.id);
		form.AddField("session_id", user.session_id);
		form.AddField("timestamp", (DateTime.Now.Ticks/10000000).ToString());
		
        UnityWebRequest www = UnityWebRequest.Post(url + scoreUrl, form);

		www.timeout = Const.TIME_OUT_SECS;
		yield return www.SendWebRequest();

		Debug.Log(www.result);

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError) {
			Debug.Log(www.error);
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

	public IEnumerator Login(string username, string password, Text errorText, IEnumerator CoroutineCallback, Callback Connected) {

		WWWForm form = new WWWForm();

		form.AddField("username", username);
		form.AddField("password", password);
		form.AddField("timelocal", (DateTime.Now.Ticks/10000000).ToString());

		UnityWebRequest www = UnityWebRequest.Post(url + loginUrl, form);
		yield return www.SendWebRequest();


		if ((www.result == UnityWebRequest.Result.ConnectionError) || (www.result == UnityWebRequest.Result.ProtocolError)) {
			Debug.Log(www.error);
			if (www.downloadHandler.text != "") {
				errorText.text = www.downloadHandler.text;
			} else {
				errorText.text = "Network error: " + www.error;
			}
			errorText.gameObject.SetActive(true);
			throw new System.Exception(www.downloadHandler.text ?? www.error);
		} else {
			Debug.Log("Form upload complete!");

			if (www.downloadHandler.text == "invalid credentials") {
				Debug.Log("invalid credentials");
				errorText.gameObject.SetActive(true);
				errorText.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = "error_invalid";
				
				yield break;
			}

			if (www.downloadHandler.text == "this account uses auth0") {
				Debug.Log("this account uses auth0");
				errorText.gameObject.SetActive(true);
				errorText.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = "error_auth0";
				yield break;
			}
			
			// Since we are connected, we can register the network status
			connected = true;

			user = JsonUtility.FromJson<UserData>(www.downloadHandler.text);			
			Debug.Log("Login json: " + www.downloadHandler.text);
			var json = SimpleJSON.JSON.Parse(www.downloadHandler.text);
			
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

		var json = SimpleJSON.JSON.Parse(www.downloadHandler.text);

		string[] words = new string[json["chosenWords"].Count];
		WordCardType[] types = new WordCardType[words.Length];

		Debug.Log("WordList json: " + www.downloadHandler.text);

		WordMaster.Instance.SetLargestModuleIndex(15);

		for (int i = 0; i < words.Length; ++i) {
			words[i] = json["chosenWords"][i];
			types[i] = (WordCardType) json["cardType"][i].AsInt;
			//Debug.Log("loop to find: " + words[i] + types[i]);
		}
		WordMaster.Instance.SetSamples(types, words);
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