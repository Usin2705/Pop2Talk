using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectManager : MonoBehaviour {

	[SerializeField] AudioInstance[] buttonEffects;
	[SerializeField] AudioInstance[] popEffects;
	[SerializeField] AudioInstance openEffect;
	[SerializeField] AudioInstance closeEffect;
	[SerializeField] AudioInstance rocketZoom;
	[SerializeField] AudioInstance rocketLand;
	[SerializeField] AudioInstance cardEnter;
	[SerializeField] AudioInstance pearlCreate;
	[SerializeField] AudioInstance music;

	static SoundEffectManager sem;

	bool musicing;

	AudioInstance musicInstance;

	void Awake() {
		sem = this;
	}

	public static SoundEffectManager GetManager() {
		return sem;
	}

	public void PlayMusic() {
		if (!musicing) {
			musicInstance = AudioMaster.Instance.Play(this, music);
			musicing = true;
		}
	}

	public void FadeMusic(float duration, float target) {
		if (musicInstance != null)
			musicInstance.FadeOut(duration, target);
	}

	public AudioInstance GetButtonSound() {
		return buttonEffects.GetRandom();
	}

	public AudioInstance GetPearlSound() {
		return pearlCreate;
	}


	public AudioInstance GetOpenSound() {
		return openEffect;
	}

	public AudioInstance GetCloseSound() {
		return closeEffect;
	}

	public AudioInstance GetRocketZoomSound() {
		return rocketZoom;
	}

	public AudioInstance GetRocketLandSound() {
		return rocketLand;
	}

	public AudioInstance GetWordCardEnterSound() {
		return cardEnter;
	}

	public AudioInstance GetPopSound() {
		return popEffects.GetRandom();
	}
}
