using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : Overlay {

	[SerializeField] CharacterSettings[] characters = null;
	[Space]
	[SerializeField] float showDuration = 0;
	[SerializeField] float waitDelay = 0;
	[SerializeField] float talkHeight = 0;
	[SerializeField] float talkSpeed = 0;
	[SerializeField] float curtainAlpha = 0;
	[Space]
	[SerializeField] Image character = null;
	[SerializeField] Image backCurtain = null;
	[SerializeField] RectTransform bottomAnchor = null;
	[SerializeField] RectTransform topAnchor = null;
	
	static CharacterManager cm;

	public CharacterSettings CurrentCharacter { get; protected set; }

	public static CharacterManager GetManager() {
		return cm;
	}

	public void Awake() {
		cm = this;
	}

	public void SetCharacter(int index, bool sendToServer = true) {
		if (index < characters.Length) {
			if (index <= -1) {
				CurrentCharacter = null;
			} else {
				CurrentCharacter = characters[index];
				character.sprite = CurrentCharacter.characterSprite;
				// TODO: maybe we should save the character, but for now we turn it off for offline version
				//if (sendToServer)					
					//NetworkManager.GetManager().UpdateCharacter(index);
			}
		} else {
			Debug.LogError("Trying to set character out of index");
		}
	}

	public Sprite[] GetCharacterSprites() {
		Sprite[] sprites = new Sprite[characters.Length];
		for (int i = 0; i < characters.Length; ++i) {
			sprites[i] = characters[i].characterSprite;
		}
		return sprites;
	}

	public void ShowCharacter(SpeechCollection speech, int order, Callback Done) {
		SetOrder(order);
		if (speech == null) {
			if (Done != null)
				Done();
			return;
		}
		StartCoroutine(ShowCharacterRoutine(speech, Done));
	}

	IEnumerator ShowCharacterRoutine(SpeechCollection speech, Callback Done) {
		int firstSpeechIndex = 0;
		AudioInstance ai = null;
		SpeechCollection.Speech s = new SpeechCollection.Speech();
		for (int i = 0; i < speech.speeches.Count; ++i) {
			s = speech.speeches[i];
			firstSpeechIndex = i;
			ai = Resources.Load<AudioInstance>(LanguageManager.GetManager().NativeLanguage.ToString() + "/" + CurrentCharacter.name + "/" + s.speech);
			if (ai == null)
				ai = Resources.Load<AudioInstance>(LanguageManager.GetManager().NativeLanguage.ToString() + "/CommonSpeeches/" + s.speech);
			if (ai != null)
				break;
		}

		if (ai == null) {
			if (Done != null)
				Done();
			yield break;
		}

		float lerp = 0;
		character.gameObject.SetActive(true);

		while (lerp < 1) {
			if (showDuration <= 0)
				lerp = 1;
			else
				lerp += Time.deltaTime / showDuration;
			character.transform.position = Vector3.Lerp(bottomAnchor.transform.position, topAnchor.transform.position, lerp);
			backCurtain.color = new Color(0, 0, 0, Mathf.Lerp(0, curtainAlpha, lerp));
			yield return null;
		}
		yield return new WaitForSeconds(waitDelay);
		float speechTimer;
		float waitTimer;
		for(int i = firstSpeechIndex; i < speech.speeches.Count; ++i) {
			if (ai == null) {
				s = speech.speeches[i];
				ai = Resources.Load<AudioInstance>(LanguageManager.GetManager().NativeLanguage.ToString() + "/" + CurrentCharacter.name + "/" + s.speech);
				if (ai == null)
					ai = Resources.Load<AudioInstance>(LanguageManager.GetManager().NativeLanguage.ToString() + "/CommonSpeeches/" + s.speech);
				if (ai == null)
					continue;
			}
			ai = AudioMaster.Instance.Play(this, ai);
			speechTimer = 0;
			waitTimer = 0;
			bool playingHasStopped = false;
			float sin;
			while (true) {
				if (ai != null && ai.IsPlaying) {
					speechTimer += Time.deltaTime * talkSpeed;
				} else {
					if (!playingHasStopped) {
						speechTimer = speechTimer % (Mathf.PI*2);
						playingHasStopped = true;
					}
				}
				if (playingHasStopped) {
					speechTimer = Mathf.MoveTowards(speechTimer, Mathf.PI*2, Time.deltaTime * talkSpeed);
					if (i < speech.speeches.Count - 1 || float.Equals(Mathf.PI*2, speechTimer))
						waitTimer += Time.deltaTime;

					if (waitTimer > s.pause) {
						break;
					}
				}
				sin = Mathf.Sin(speechTimer);
				character.transform.position = Vector3.up * Mathf.Sign(sin) * Mathf.Pow(Mathf.Abs(sin), 0.75f) * talkHeight + topAnchor.transform.position;
				yield return null;
			}
			ai = null;
		}
		if (speech.hideAtEnd) {
			lerp = 0;
			while (lerp < 1) {
				if (showDuration <= 0)
					lerp = 1;
				else
					lerp += Time.deltaTime / showDuration;
				character.transform.position = Vector3.Lerp(topAnchor.transform.position, bottomAnchor.transform.position, lerp);
				backCurtain.color = new Color(0, 0, 0, Mathf.Lerp(curtainAlpha, 0, lerp));
				yield return null;
			}
			HideCharacter();
		}
		if (Done != null)
			Done();
	}

	public void HideCharacter() {
		backCurtain.color = new Color(0, 0, 0, 0);
		character.transform.position = bottomAnchor.transform.position;
		character.gameObject.SetActive(false);
	}
}