using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WordCardManager : MonoBehaviour {

	static WordCardManager wordCardManager;
	[SerializeField]
	BaseWordCard wordCard;
	[SerializeField]
	AudioInstance recordSound;

	[SerializeField]
	LanguagePromptDictionary quizPrompts;
	[SerializeField]
	LanguageAudioClipDictionary memoryPrompts;
	[Space]
	[SerializeField] SpeechCollection[] starSpeeches;
	[SerializeField] SpeechCollection timeOutSpeech;
    [Space]
    [SerializeField] [Range(0.0f, 1.0f)] float cheerChance = 0.25f;

    Queue<WordCardType> cardQueue;

	enum WordCardAction { None, Retry, Continue };
	WordCardAction action = WordCardAction.None;

	AudioSource audioSource;

	BaseWordCardHandler cardHandler;

	WordData currentWord;
	WordData nextWord;
	WordCardType nextWordType;
	WordData nonNextWord;

	float introDuration = 0.75f;
	float hideDuration = 1.5f;
	int recordDuration = 3;
	float starDuration = 0.3f;
	float micExtra = 0.15f;

    bool firstCheerDone = false;

	int score = 0;
	bool scoreReceived = false;

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
		if (DebugSettings.Instance.skipTransitions)
			wordCard.RevealInstantly();
		else
			yield return wordCard.ShowCard(introDuration);
		IntroDone();
	}

	public void SetUpWord(WordData data, string levelName, BaseWordCardHandler cardHandler) {
		currentWord = data;
		this.cardHandler = cardHandler;
		this.levelName = levelName;
	}

	public void SetQuiz(bool quiz) {
		wordCard.SetQuiz(quiz);
	}

	public void SetUpCard(Language language) {
		action = WordCardAction.None;
		wordCard.SetSpelling(currentWord.languageWords[language].spelling);
		wordCard.SetPicture(currentWord.picture);
		wordCard.SetFlagOneOn(false);
		wordCard.SetFlagTwoOn(false);
		wordCard.gameObject.SetActive(true);
		wordCard.ToggleFeedback(false);
		wordCard.ToggleMic(false);
		wordCard.ToggleButtons(false);
		wordCard.SetStars(0);
	}

	public IEnumerator StartingAnimation() {
		yield return wordCard.StartingAnimation();
	}

	/// <summary>
	/// Vanilla
	/// </summary>
	/// <param name="language"></param>
	/// <returns></returns>
	public IEnumerator SayWord(Language language) {
		audioSource.clip = currentWord.languageWords[language].pronunciations.GetRandom();
		wordCard.SetSpelling(currentWord.languageWords[language].spelling);
		if (audioSource.clip == null)
			yield break;
		audioSource.Play();
		NetworkManager.GetManager().SamplePlayed(levelName, currentWord.name, false);
		yield return new WaitForSeconds(audioSource.clip.length);
	}

	/// <summary>
	/// No sample pronunciation?
	/// </summary>
	/// <param name="native"></param>
	/// <param name="target"></param>
	/// <returns></returns>
	public IEnumerator QuizPrompt(Language native, Language target) {
		audioSource.clip = quizPrompts[native].prompts[target];
		if (audioSource.clip != null) {
			audioSource.Play();
			yield return new WaitForSeconds(audioSource.clip.length);
		}
	}

	/// <summary>
	/// Small pause between sample pronunciation and recording
	/// </summary>
	/// <param name="native"></param>
	/// <param name="waitDuration"></param>
	/// <returns></returns>
	public IEnumerator MemoryPrompt(Language native, float waitDuration) {
		audioSource.clip = memoryPrompts[native];
		audioSource.Play();
		yield return new WaitForSeconds(audioSource.clip.length);
		wordCard.Wait(waitDuration);
	}


	public void SetFlags(bool native, bool target) {
		wordCard.SetFlagOneOn(native);
		wordCard.SetFlagTwoOn(target);
	}

	public IEnumerator RecordAndPlay(float gap, string challengeType) {
		score = 0;
		wordCard.ToggleMic(true);
		/*if (recordSound != null) {
			AudioInstance record = AudioMaster.Instance.Play(this, recordSound);
			yield return new WaitForSecondsRealtime(record.GetLength());
		}*/
		AudioClip recording = Microphone.Start(Microphone.devices[0], false, recordDuration, 16000);
		yield return new WaitForSeconds(micExtra);

		string word = "";
		string[] parts = currentWord.name.Split(' ', '_');
		for (int i = 0; i < parts.Length; ++i) {
			if (i != 0)
				word += " ";
			word += LanguageManager.GetManager().GetLanguagePrefix() + parts[i];
		}

		NetworkManager.GetManager().SendMicrophone(Microphone.devices[0], word, recording, recordDuration, ReceiveScore, challengeType);
		bool enoughRecording;
		scoreReceived = false;
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
		yield return new WaitForSeconds(gap);
		audioSource.Play();
		yield return new WaitForSeconds(audioSource.clip.length);
	}

	public void ReceiveScore(int score) {
		this.score = score;
		scoreReceived = true;
	}


	public IEnumerator GiveStars(float phaseGap) {
		bool timeOut = false;
		if (NetworkManager.GetManager().Connected && !scoreReceived) {
			NetworkManager.GetManager().ServerWait(true);
			float timer = NetworkManager.GetManager().TimeoutDuration;
			while (NetworkManager.GetManager().Connected && !scoreReceived) {
				timer -= Time.deltaTime;
				if (timer < 0) {
					timeOut = true;
					break;
				}
				yield return null;
			}
			NetworkManager.GetManager().ServerWait(false);
		}
		if (!timeOut && NetworkManager.GetManager().Connected) {
			score = Mathf.Max(0, score);
			wordCard.SetOnlineStars();
		} else {
			score = Random.Range(1, 5);
			wordCard.SetOfflineStars();
		}
		cardHandler.SetStars(score);
		yield return wordCard.SetStars(0, 0f);
		yield return wordCard.SetStars(score, starDuration);
		yield return new WaitForSeconds(phaseGap);
		yield return wordCard.SetStars(cardHandler.GetStars(), 0f);
		SpeechCollection speech = !firstCheerDone ? starSpeeches[Random.Range(0, starSpeeches.Length)] : ( Random.Range(0,1) >= cheerChance ? starSpeeches[Random.Range(0, starSpeeches.Length)] : null);
        if (speech != null && !firstCheerDone)
            firstCheerDone = true;
		if (timeOut /*|| (score == 0 && NetworkManager.GetManager().Connected)*/)
			speech = null;
		if (!DebugSettings.Instance.skipTransitions && CharacterManager.GetManager().CurrentCharacter != null)
			CharacterManager.GetManager().ShowCharacter(speech, wordCard.GetOrder(), () => { StartCoroutine(FinishCard()); });
		else
			StartCoroutine(FinishCard());
	}

	public IEnumerator FinishCard(bool feedback = true) {
		yield return wordCard.FinishingAnimation();
		wordCard.ToggleFeedback(feedback);
		wordCard.ToggleButtons(true);
		while (action == WordCardAction.None)
			yield return null;
		if (action == WordCardAction.Continue)
			cardHandler.CardDone();
		else
			cardHandler.Retry();
	}

	public void Retry() {
		AudioMaster.Instance.Play(this, SoundEffectManager.GetManager().GetButtonSound());
		action = WordCardAction.Retry;
	}

	public void Continue() {
		AudioMaster.Instance.Play(this, SoundEffectManager.GetManager().GetButtonSound());
		action = WordCardAction.Continue;
	}

	public void HideCard(Callback Done, bool instantly = false) {
		if (instantly || DebugSettings.Instance.skipTransitions) {
			wordCard.HideInstantly();
			Done?.Invoke();
		} else
			wordCard.HideCard(hideDuration, Done);
	}
}
