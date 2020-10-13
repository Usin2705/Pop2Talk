using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectManager : MonoBehaviour {

	[SerializeField] AudioInstance[] buttonEffects = null;
	[SerializeField] AudioInstance[] popEffects = null;
	[SerializeField] AudioInstance openEffect = null;
	[SerializeField] AudioInstance closeEffect = null;
	[SerializeField] AudioInstance rocketZoom = null;
	[SerializeField] AudioInstance rocketLand = null;
	[SerializeField] AudioInstance cardEnter = null;
	[SerializeField] AudioInstance pearlCreate = null;
	[SerializeField] AudioInstance music = null;
	[SerializeField] AudioInstance badClick = null;

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

	public AudioInstance GetBadClickSound() {
		return badClick;
	}
}
