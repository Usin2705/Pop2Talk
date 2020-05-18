using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioInstance : MonoBehaviour, IPoolable {

    Transform follow;
    Vector3 offset;
    MonoBehaviour conductor;
    Quaternion startRotation;
    AudioSource aus;

    float timer = 0;
    float basePitch;
    float baseVolume;
    bool playing;

    bool isSet;

	bool fading;
	float targetVolume;
	float duration;
	float playingVolume;

    public bool IsSet {
        get {
            return isSet;
        }

        set {
            isSet = value;
        }
    }

	public bool IsPlaying { get { return playing; } }

    public void Play(MonoBehaviour conductor, Transform follow, Vector3 offset, float pitch, float volume) {
        if (aus == null) {
            aus = GetComponent<AudioSource>();
            basePitch = aus.pitch;
            baseVolume = aus.volume;
        }
        this.conductor = conductor;
        this.follow = follow;
        if (follow == null) {
            transform.position = offset;
        } else {
            startRotation = follow.rotation;
            this.offset = Quaternion.Inverse(startRotation) * offset;
        }
        aus.pitch = basePitch * pitch;
        aus.volume = baseVolume * volume;
		playingVolume = aus.volume;
        aus.Play();
        timer = 0;
        playing = true;
    }

    void Update() {
        if (aus == null)
            return;
		if (playing) {
			timer += Time.deltaTime * Mathf.Abs(aus.pitch);
		}
		if (fading) {
			aus.volume = Mathf.MoveTowards(aus.volume, targetVolume * playingVolume, Time.deltaTime * playingVolume / (duration));
			if (aus.volume == targetVolume)
				fading = false;
		}
        if (timer >= aus.clip.length) {
            if (aus.loop) {
                timer -= aus.clip.length;
            } else {
                Stop();
            }
        }
    }

    void LateUpdate() {
        if (follow != null) {
            transform.position = follow.position + follow.rotation * offset;
        }
    }

    public void OnReturnToPool() {
        if (IsSet)
            Destroy(gameObject);
    }

    public void SetClip(AudioClip clip) {
        if (aus == null) {
            aus = GetComponent<AudioSource>();
            basePitch = aus.pitch;
            baseVolume = aus.volume;
        }
        aus.clip = clip;
        IsSet = true;
    }

    public float GetLength() {
        if (aus == null)
            aus = GetComponent<AudioSource>();
        if(aus == null || aus.clip == null)
            return 0;
        return aus.clip.length;
    }

    public void Stop(MonoBehaviour conductor) {
        if (this.conductor == conductor)
            Stop();
    }

    void Stop() {
        aus.Stop();
        playing = false;
        conductor = null;
        PoolMaster.Instance.Destroy(gameObject);
    }

	public void FadeOut(float duration, float target) {
		fading = true;
		this.duration = duration;
		this.targetVolume = target;
	}
}
