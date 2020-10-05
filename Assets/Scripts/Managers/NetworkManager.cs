using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.UI;
using System.Reflection;
using socket.io;

public class NetworkManager : MonoBehaviour {

	static NetworkManager netWorkManager;

	string url = "https://(user_management_api_server)/";

	string login = "api/game/login";
	string coinUpdate = "api/game/update/coins";
	string updateCharacter = "api/game/update/character";
	string updateHighscore = "api/game/update/highscore";
	string getWordList = "api/game/create/wordlist";
	string unlockCosmetic = "api/game/unlock/cosmetic";
	string equipCosmetic = "api/game/equip/cosmetic";

	string devAccount = "devel";

	string liveSocketUrl = "https://(speech_processing_production_server)/";
	string devSocketUrl = "https://(speech_processing_development_server)/";

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
	float mu = 255f;

	// datatype can be:
	//  'int16'  16 bit signed integer (short) 'h'
	//   'int8'   8 bit signed char    (sbyte) 'b'
	string datatype = "int8";

	float time;
	float connectAttemptTime;
	float checkOkTime;

	bool connected = false;
	bool connecting = false;
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

	public void Connect() {
		connecting = true;
		connectAttemptTime = Time.time;
		attemptingToConnect = true;
		socket = Socket.Connect(GetSocketUrl());
		Debug.Log(socket.Namespace);
		Debug.Log("Connecting");
		CheckConnection();
	}

	public static NetworkManager GetManager() {
		return netWorkManager;
	}

	void OnDestroy() {

	}

	public void ServerWait(bool wait) {
		if (wait && !serverWaitPromptActive) {
			serverWaitPromptActive = true;
			DataOverlayManager.GetManager().Show(99, false, "Waiting for server...", null, null, null, null, null);
		} else if (serverWaitPromptActive) {
			DataOverlayManager.GetManager().Close();
			serverWaitPromptActive = false;
		}
	}

	void CheckConnection() {
		socket.Emit("are_you_ok");
		socket.On(SystemEvents.connect, () => {
			Debug.Log("Connected successfully");
			connected = true;
			connecting = false;
			SimpleEvent("start");
			SendAccessTokenToSocket(user.access_token, user.username);
		});

		/*socket.On(Socket.EVENT_CONNECT_ERROR, () =>
        {
            Debug.Log("Connect error");
            connecting = false;

        });
        socket.On(Socket.EVENT_CONNECT_TIMEOUT, () =>
        {
            Debug.Log("Connect timeout");
            connecting = false;

        });
        socket.On(Socket.EVENT_ERROR, () =>
        {
            Debug.Log("error");
            connecting = false;

        });*/
		socket.On(SystemEvents.disconnect, () => {
			connected = false;
			Debug.Log("Disconnect");
			connecting = false;

		});

		socket.On("i_am_ok", () => {
			connected = true;
			Debug.Log("Connected: " + (time - connectAttemptTime));
			connecting = false;
		});
		/*
        socket.On(Socket.EVENT_RECONNECT_ERROR, () =>
        {
            connected = false;
            Debug.Log("Reconnect error");
            connecting = false;

        });
        socket.On(Socket.EVENT_RECONNECT_FAILED, () =>
        {
            connected = false;
            Debug.Log("Reconnect failed");
            connecting = false;

        });
        socket.On(Socket.EVENT_RECONNECT_ATTEMPT, () =>
        {
            Debug.Log("Reconnect attempt");
            reconnecting = true;
            connecting = false;

        });*/
		socket.On(SystemEvents.reconnect, (int reconnectAttempt) => {
			Debug.Log("Reconnect " + reconnectAttempt);
			connected = true;
			connecting = false;
		});
		/*
        socket.On(Socket.EVENT_RECONNECTING, () =>
        {
            Debug.Log("Reconnecting");
            reconnecting = true;
            connecting = false;

        });*/

	}

	/*public void StopTryingToConnect()
    {
        if (!connected && attemptingToConnect)
        {
            attemptingToConnect = false;
            socket.Off("i_am_ok");
            //socket.Disconnect();
            socket = null;
        }
    }*/

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

	public void LevelCompleted(string level, bool passed, bool medal) {
		if (socket != null && connected) {
			LevelEvent le = new LevelEvent();
			le.player = Player;
			le.level = level;
			le.passed = passed;
			le.medal = medal;
			socket.EmitJson("level_complete", JsonUtility.ToJson(le));
		}
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
		if (socket != null && connected)
			StartCoroutine(UploadMicrophone(microphone, word, clip, duration, ScoreReceived, challengetype, retryAmount));
	}

	IEnumerator UploadMicrophone(string microphone, string word, AudioClip clip, float duration, IntCallback ScoreReceived, string challengetype, int retryAmount) {
		while (!connected || waitingScore)
			yield return null;
		waitingScore = true;
		int packets = 0;
		int packetSize = Mathf.RoundToInt(fs * packetGap);
		float[] rawSamples = new float[packetSize];
		// samples array should be same datatype as used for transfer:
		switch (datatype) {
			case "int16":
				samplesShort = new short[packetSize];
				voiceData = new byte[2 * packetSize];
				break;
			case "int8":
				samplesSbyte = new sbyte[packetSize];
				voiceData = new byte[1 * packetSize];
				break;
		}
		string token = "Mitä tähän halutaan? Tässä randomia: " + UnityEngine.Random.value;
		InitialAudioData sud = new InitialAudioData();
		EstablishedAudioData ud = new EstablishedAudioData();
		float a = 0;
		socket.On("score", (string data) => {
			waitingScore = false;
			var json = SimpleJSON.JSON.Parse(data);
			Debug.Log(json);
			ScoreReceived(json["stars"].AsInt);
			//ScoreReceived((int)((JObject)JsonConvert.DeserializeObject((string)data))["score"]);
			//Debug.Log("Score received: " + (int)((JObject)JsonConvert.DeserializeObject((string)data))["score"]);
			socket.Off("score");
		});
		Debug.Log("Starting sound upload");
		while (packets < Mathf.CeilToInt(duration / packetGap)) {
			if (Microphone.GetPosition(microphone) >= packetSize + packetSize * packets) {
				clip.GetData(rawSamples, packetSize * packets);

				switch (datatype) {
					case "int16":
						for (int i = 0; i < rawSamples.Length; ++i) {
							samplesShort[i] = (short)(short.MaxValue * rawSamples[i]);
						}
						Buffer.BlockCopy(samplesShort, 0, voiceData, 0, voiceData.Length);
						break;
					case "int8":
						for (int i = 0; i < rawSamples.Length; ++i) {
							// Decoding in Python:
							// np.sign(compressed_val)*(1/mu)*(np.power(1+mu, np.abs(compressed_val)/127)-1).tolist()
							// So encoding is: round (sign * ln(1 + mu *abs(x) / ln(1+mu) * 127)  
							samplesSbyte[i] = (sbyte)Math.Round(
							Math.Sign(rawSamples[i]) * sbyte.MaxValue * (Math.Log(1 + 255 * Math.Abs(rawSamples[i])) / Math.Log(1 + 255))
											);
						}
						Buffer.BlockCopy(samplesSbyte, 0, voiceData, 0, voiceData.Length);
						break;
				}


#if UNITY_STANDALONE_WIN || UNITY_EDITOR
				/*
                if (packets == 0)
                {
                    sud.player = Player;
                    sud.word = word;
                    sud.gameversion = "0.2";
                    sud.authenticationtoken = token;
                    sud.dataencoding = "base64";
                    // Add the data encoding and data types: 
                    sud.datatype = datatype;
                    sud.dataencoding = dataencoding;

                    // I'd like to have the following here, is that easily done?:
                    
                    sud.gameversion = "";
                    sud.build = "";
                    sud.builddate = "";
                    sud.device = "";
                    sud.microphone = microphone;
                    sud.challengetype = "";
                    sud.level = "";

                    sud.packetnr = packets;
                    sud.data = System.Convert.ToBase64String(voiceData);
                    socket.Emit("start_upload", JsonConvert.SerializeObject(sud));
                }
                else if (packets == Mathf.CeilToInt(duration / packetGap) - 1)
                {
                    ud.player = Player;
                    ud.authenticationtoken = token;
                    ud.packetnr = packets;
                    ud.data = System.Convert.ToBase64String(voiceData);
                    socket.Emit("finish_upload", JsonConvert.SerializeObject(ud));
                }
                else
                {
                    ud.player = Player;
                    ud.authenticationtoken = token;
                    ud.packetnr = packets;
                    ud.data = System.Convert.ToBase64String(voiceData);
                    socket.Emit("continue_upload", JsonConvert.SerializeObject(ud));
                }*/
#endif

				//#if UNITY_ANDROID || UNITY_IOS
				if (packets == 0) {
					sud.player = Player;
					sud.word = word;
					sud.gameversion = "0.2";
					sud.authenticationtoken = token;
					sud.dataencoding = "base64";
					// Add the data encoding and data types: 
					sud.datatype = datatype;
					sud.dataencoding = dataencoding;

					// I'd like to have the following here, is that easily done?:
					//sud.gameversion = "";
					sud.build = Assembly.GetExecutingAssembly().GetName().Version.Build.ToString();

					System.Version version = Assembly.GetExecutingAssembly().GetName().Version;
					System.DateTime startDate = new System.DateTime(2000, 1, 1, 0, 0, 0);
					System.TimeSpan span = new System.TimeSpan(version.Build, 0, 0, version.Revision * 2);
					System.DateTime buildDate = startDate.Add(span);
					sud.builddate = buildDate;
					sud.device = SystemInfo.deviceName.ToString();
					sud.microphone = microphone;
					sud.challengetype = challengetype;
					sud.retryCount = retryAmount;
					//sud.level = "";

					sud.packetnr = packets;
					sud.data = System.Convert.ToBase64String(voiceData);
					socket.EmitJson("start_upload", JsonUtility.ToJson(sud));
				} else if (packets == Mathf.CeilToInt(duration / packetGap) - 1) {
					ud.player = Player;
					ud.authenticationtoken = token;
					ud.packetnr = packets;
					ud.data = System.Convert.ToBase64String(voiceData);
					socket.EmitJson("finish_upload", JsonUtility.ToJson(ud));
				} else {
					ud.player = Player;
					ud.authenticationtoken = token;
					ud.packetnr = packets;
					ud.data = System.Convert.ToBase64String(voiceData);
					socket.EmitJson("continue_upload", JsonUtility.ToJson(ud));
				}
				//#endif
				packets++;
			} else {
				a += Time.deltaTime;
				yield return null;
			}
		}
		Debug.Log("Record duration: " + a);
	}

	/* public UserData Login(string username, string password)
     {

         StartCoroutine(SendLoginRequest(username, password));

         return user;
     }*/


	public IEnumerator Login(string username, string password, Text errorText, IEnumerator CoroutineCallback, Callback Connected) {

		WWWForm form = new WWWForm();

		form.AddField("username", username);
		form.AddField("password", password);

		UnityWebRequest www = UnityWebRequest.Post(url + login, form);
		yield return www.SendWebRequest();


		if (www.isNetworkError || www.isHttpError) {
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
				errorText.text = "Invalid username/password";
				errorText.gameObject.SetActive(true);
				yield break;
			}

			if (www.downloadHandler.text == "this account uses auth0") {
				Debug.Log("this account uses auth0");
				errorText.text = "User account not found. Please log in using Google";
				errorText.gameObject.SetActive(true);
				yield break;
			}

			user = JsonUtility.FromJson<UserData>(www.downloadHandler.text);
			Debug.Log(www.downloadHandler.text);
			var json = SimpleJSON.JSON.Parse(www.downloadHandler.text);
			
			WordMaster.Instance.ClearWords();

			for (int i = 0; i < json["words"].Count; ++i) {
				WordMaster.Instance.AddWord(json["words"][i]["word"].Value);
			}

			for (int i = 0; i < json["game_state"]["wordHighscores"].Count; ++i) {
				WordMaster.Instance.SetStarAmount(json["game_state"]["wordHighscores"][i]["word"].Value, json["game_state"]["wordHighscores"][i]["maxstars"].AsInt);
			}

			int module = 1;
			for (int i = 0; i < json["game_state"]["availableModules"].Count; ++i) {
				module = Mathf.Max(module, json["game_state"]["availableModules"][i]["module"].AsInt);
			}

			json["game_state"]["availableModules"] = "";
			json["game_state"]["wordHighscores"] = "";
			json["words"] = "";

			Debug.Log(json);

			WordMaster.Instance.SetLargestModuleIndex(module);

			if (json["game_state"]["character"].ToString() != "")
				CharacterManager.GetManager().SetCharacter(json["game_state"]["character"].AsInt, false);

			CurrencyMaster.Instance.SetCoins(json["game_state"]["coins"].AsInt);
			
			if (json["game_state"]["unlocked_cosmetics"].ToString() != "") {
				for (int i = 0; i < json["game_state"]["unlocked_cosmetics"].Count; ++i) {
					CosmeticManager.GetManager().UnlockCosmetic(json["game_state"]["unlocked_cosmetics"][i], false);
				}
			}

			if (json["game_state"]["equipped_cosmetics"].ToString() != "") {
				for (int i = 0; i < json["game_state"]["equipped_cosmetics"].Count; ++i) {
					CosmeticManager.GetManager().EquipCosmetic(json["game_state"]["equipped_cosmetics"][i], false);
				}
			}

			CosmeticManager.GetManager().CheckDefaultCosmetics();

			if (user.consent) {
				Connect();
				StartCoroutine(CoroutineCallback);
			} else {
				Connected();
			}
		}
	}

	public void UpdateCoins(int coins) {
		StartCoroutine(CoinUpdateRoutine(coins));
	}

	IEnumerator CoinUpdateRoutine(int coins) {

		WWWForm form = new WWWForm();
		form.AddField("coins", coins);
		UnityWebRequest www = UnityWebRequest.Post(url + coinUpdate, form);
		www.SetRequestHeader("Authorization", "Bearer " + user.access_token);
		yield return www.SendWebRequest();
	}

	public void GetWordList(Callback Done) {
		StartCoroutine(WordListRoutine(Done));
	}

	IEnumerator WordListRoutine(Callback Done) {

		WWWForm form = new WWWForm();
		UnityWebRequest www = UnityWebRequest.Post(url + getWordList, form);
		www.SetRequestHeader("Authorization", "Bearer " + user.access_token);
		yield return www.SendWebRequest();
		Debug.Log(www.downloadHandler.text);
		var json = SimpleJSON.JSON.Parse(www.downloadHandler.text);

		string[] words = new string[json["chosenWords"].Count];
		WordCardType[] types = new WordCardType[words.Length];

		WordMaster.Instance.SetLargestModuleIndex(json["module"].AsInt);

		for (int i = 0; i < words.Length; ++i) {
			words[i] = json["chosenWords"][i];
			types[i] = (WordCardType) json["cardType"][i].AsInt;
		}
		WordMaster.Instance.SetSamples(types, words);
		Done?.Invoke();
	}

	public void UpdateWordScore(string word, int score, int challenge) {
		StartCoroutine(WordScoreUpdateRoutine(word, score, challenge));
	}

	IEnumerator WordScoreUpdateRoutine(string word, int score, int challenge) {

		WWWForm form = new WWWForm();
		form.AddField("word", word);
		form.AddField("score", score);
		form.AddField("challenge", challenge);
		UnityWebRequest www = UnityWebRequest.Post(url + updateHighscore, form);
		www.SetRequestHeader("Authorization", "Bearer " + user.access_token);
		yield return www.SendWebRequest();
	}

	public void UpdateCharacter(int character) {
		StartCoroutine(CharacterUpdateRoutine(character));
	}

	IEnumerator CharacterUpdateRoutine(int character) {
		WWWForm form = new WWWForm();
		form.AddField("character", character);
		UnityWebRequest www = UnityWebRequest.Post(url + updateCharacter, form);
		www.SetRequestHeader("Authorization", "Bearer " + user.access_token);
		yield return www.SendWebRequest();
		Debug.Log(www.downloadHandler.text);
	}
	
	public void UnlockCosmetic(string id) {
		Debug.Log("Trying to unlock: " + id);
		StartCoroutine(CosmeticUnlockRoutine(id));
	}

	IEnumerator CosmeticUnlockRoutine(string id) {
		WWWForm form = new WWWForm();
		form.AddField("id", id);
		UnityWebRequest www = UnityWebRequest.Post(url + unlockCosmetic, form);
		www.SetRequestHeader("Authorization", "Bearer " + user.access_token);
		yield return www.SendWebRequest();
		Debug.Log(www.downloadHandler.text);
	}

	public void EquipCosmetic(string id, int index) {
		Debug.Log("Trying to equip: " + id);
		StartCoroutine(CosmeticEquipRoutine(id, index));
	}

	IEnumerator CosmeticEquipRoutine(string id, int index) {
		WWWForm form = new WWWForm();
		form.AddField("id", id);
		form.AddField("index", index);
		UnityWebRequest www = UnityWebRequest.Post(url + equipCosmetic, form);
		www.SetRequestHeader("Authorization", "Bearer " + user.access_token);
		yield return www.SendWebRequest();
		Debug.Log(www.downloadHandler.text);
	}
	
	public bool GetConsent() {
		if (user != null)
			return user.consent;
		return false;
	}

	void Reconnect() {
		Debug.Log("Reconnect confirm");
		connected = false;
		reconnecting = false;
		//socket.Disconnect();
		socket = null;
		Connect();
	}

	void SendAccessTokenToSocket(string token, string username) {
		socket.EmitJson("auth", "{ " +
			"\"token\":" + "\"" + token + "\"," +
			"\"user\":" + "\"" + username + "\" }");
	} 

	public void SimpleEvent(string name) {


		AnalyticsEvent ae = new AnalyticsEvent {
			eventname = name,
			player = Player,
			sessionid = sessionId,
		};
		StartCoroutine(SendLoggableEvent(ae));
	}

	public void CharacterSelectEvent(string name, string character) {


		AnalyticsEvent ae = new AnalyticsEvent {
			eventname = name,
			eventtarget = character,
			player = Player,
			sessionid = sessionId,
		};
		StartCoroutine(SendLoggableEvent(ae));
	}

	public void LevelCompleteEvent(string name, string stageName, string stageType, float levelDuration, float averageStars, int totalStarsCollected, int stonesCollected) {


		AnalyticsEvent ae = new AnalyticsEvent {
			eventname = name,
			avgStars = averageStars,
			totalStars = totalStarsCollected,
			stones = stonesCollected,
			leveltime = levelDuration,
			level = stageName,
			levelType = stageType,
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
		StartCoroutine(SendLoggableEvent(ae));
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

		ae.serverurl = GetSocketUrl();

		socket.EmitJson("loggable_event", JsonUtility.ToJson(ae));
		yield return null;

		Debug.Log("Event " + ae.eventname + " sent");
	}

	string GetSocketUrl() {
		if (user.username.ToLower().StartsWith(devAccount.ToLower()) && !user.username.Contains("@"))
			return devSocketUrl;
		return liveSocketUrl;
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