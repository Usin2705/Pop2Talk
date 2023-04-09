using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WordCardManager : MonoBehaviour {

	static WordCardManager wordCardManager;

	[SerializeField] BaseWordCard wordCard = null;
	
	[Space]
	[SerializeField] SpeechCollection[] starSpeeches = null;
    [Space]
    [SerializeField] [Range(0.0f, 1.0f)] float cheerChance = 0.25f;
	[SerializeField] int backUpStar = 5;

    Queue<WordCardType> cardQueue;

	enum WordCardAction { None, Retry, Continue };
	WordCardAction action = WordCardAction.None;

	AudioSource audioSource;

	BaseWordCardHandler cardHandler;

	WordData currentWord;

	float introDuration = 0.75f;
	float hideDuration = 1.5f;
	int recordDuration = 3;
	float starDuration = 0.3f;
	float micExtra = 0.15f;

	int retryCount = 0;

    bool firstCheerDone = false;

	int stars = 0;
	int prevHighScore;
	bool starsReceived = false;

	string levelName;

	void Awake() {
		if (wordCardManager != null) {
			Debug.LogError("Multiple WordCardManagers");
			Destroy(gameObject);
			return;
		}
		audioSource = GetComponent<AudioSource>();
		wordCardManager = this;
	}

	public static WordCardManager GetManager() {
		return wordCardManager;
	}

	public void StopCard() {
        if (Microphone.devices[0] != null)
        {
            if (Microphone.IsRecording(Microphone.devices[0]))
            {
                Microphone.End(Microphone.devices[0]);
            }
        }
		StopAllCoroutines();
		wordCard.StopCard();
	}

	public void ShowWordCard(int order, Callback CardFinishCallBack) {
		StartCoroutine(CardIntro(order, CardFinishCallBack));
	}

	IEnumerator CardIntro(int order, Callback IntroDone) {
		wordCard.SetOrder(order);
		if (DebugMaster.Instance.skipTransitions)
			wordCard.RevealInstantly();
		else
			yield return wordCard.ShowCard(introDuration);
		IntroDone();
	}

	public void SetUpWord(WordData data, string levelName, BaseWordCardHandler cardHandler) {
		retryCount = 0;
		currentWord = data;
		prevHighScore = WordMaster.Instance.GetHighScore(data.name);
		this.cardHandler = cardHandler;
		this.levelName = levelName;
	}

	public void SetMemory(bool memory) {
		wordCard.SetMemory(memory);
	}

	public void SetUpCard() {
		action = WordCardAction.None;
		wordCard.SetPicture(currentWord.picture);
		wordCard.gameObject.SetActive(true);
		wordCard.ToggleMic(false);
		wordCard.ToggleButtons(false);
		wordCard.SetStars(0);
	}

	public IEnumerator StartingAnimation() {
		yield return wordCard.StartingAnimation();
	}
	
	public IEnumerator SayWord() {
		audioSource.clip = currentWord.pronunciations.GetRandom();
		if (audioSource.clip == null)
			yield break;
		audioSource.Play();
		NetworkManager.GetManager().SamplePlayed(levelName, currentWord.name, false);
		yield return new WaitForSeconds(audioSource.clip.length);
	}
	

	public IEnumerator RecordAndPlay(float gap, string challengeType) {
		stars = 0;
		wordCard.ToggleMic(true);
		AudioClip recording = Microphone.Start(Microphone.devices[0], false, recordDuration, 16000);
		yield return new WaitForSeconds(micExtra);

		bool enoughRecording;
		starsReceived = false;
		float a = 0;
		float[] samples = new float[512];
		int offset = -samples.Length;
		while (a < recordDuration) {
			enoughRecording = false;
			a += Time.deltaTime;
			while (Microphone.GetPosition(Microphone.devices[0]) - offset >= samples.Length * 2) {
				enoughRecording = true;
				offset += samples.Length;
			}
			if (enoughRecording) {
				recording.GetData(samples, offset);
				wordCard.VisualizeAudio(samples);
			}
			yield return null;
		}
		audioSource.clip = recording;
		wordCard.ToggleMic(false);
		NetworkManager.GetManager().SendMicrophone(Microphone.devices[0], currentWord.name, recording, recordDuration, ReceiveStars, challengeType, retryCount);
		
		yield return new WaitForSeconds(gap);
		audioSource.Play();
		yield return new WaitForSeconds(audioSource.clip.length);
	}

	public void ReceiveStars(int stars) {
		Debug.Log("Received stars:" + stars);
		this.stars = stars;
		starsReceived = true;
	}


	public IEnumerator GiveStars(float phaseGap) {
		bool timeOut = false;
		if (NetworkManager.GetManager().Connected && !starsReceived) {
			NetworkManager.GetManager().ServerWait(true);
			float timer = NetworkManager.GetManager().TimeoutDuration;
			while (NetworkManager.GetManager().Connected && !starsReceived) {
				timer -= Time.deltaTime;
				if (timer < 0) {
					timeOut = true;
					break;
				}
				yield return null;
			}
			NetworkManager.GetManager().ServerWait(false);
		}		

		// If we are online, we want to use the stars we received from the server
		// However, we do not use socket but resful api
		// So the check for connection may be not necessary???		
		if (!timeOut && NetworkManager.GetManager().Connected) {
			stars = Mathf.Max(0, stars);
			wordCard.SetOnlineStars();
		} else {
			stars = backUpStar;
			wordCard.SetOfflineStars();
		}
		cardHandler.SetStars(stars);
		yield return wordCard.SetStars(0, 0f);
		yield return wordCard.SetStars(stars, starDuration);
		yield return new WaitForSeconds(phaseGap);
		yield return wordCard.SetStars(cardHandler.GetStars(), 0f);
		SpeechCollection speech = !firstCheerDone ? starSpeeches[Random.Range(0, starSpeeches.Length)] : ( Random.Range(0,1) >= cheerChance ? starSpeeches[Random.Range(0, starSpeeches.Length)] : null);
        if (speech != null && !firstCheerDone)
            firstCheerDone = true;
		if (timeOut)
			speech = null;
		if (!DebugMaster.Instance.skipTransitions && CharacterManager.GetManager().CurrentCharacter != null)
			CharacterManager.GetManager().ShowCharacter(speech, wordCard.GetOrder(), () => { StartCoroutine(FinishCard()); });
		else
			StartCoroutine(FinishCard());
	}

	public IEnumerator FinishCard(bool feedback = true) {
		yield return wordCard.FinishingAnimation();
		wordCard.ToggleButtons(true);
		while (action == WordCardAction.None)
			yield return null;
		if (action == WordCardAction.Continue) {
			cardHandler.CardDone();
		} else
			cardHandler.Retry();
	}

	public void Retry() {
		retryCount++;
		AudioMaster.Instance.Play(this, SoundEffectManager.GetManager().GetButtonSound());
		action = WordCardAction.Retry;
	}

	public void Continue() {
		AudioMaster.Instance.Play(this, SoundEffectManager.GetManager().GetButtonSound());
		action = WordCardAction.Continue;
	}

	public void HideCard(Callback Done, bool instantly = false) {
		if (instantly || DebugMaster.Instance.skipTransitions) {
			wordCard.HideInstantly();
			Done?.Invoke();
		} else {
			wordCard.HideCard(hideDuration, Done, stars > prevHighScore, currentWord);
		}
	}
}
