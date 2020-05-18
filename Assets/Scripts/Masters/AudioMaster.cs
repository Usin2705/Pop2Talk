using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioMaster {

    static AudioMaster instance;
    public static AudioMaster Instance {
        get {
            if (instance == null) {
                instance = new AudioMaster();
            }

            return instance;
        }
    }

	AudioInstance empty;

    public AudioInstance InstancifyClip(AudioClip clip) {
		if (empty == null)
			 empty = (Resources.Load("EmptyAudio") as GameObject).GetComponent<AudioInstance>();
		empty.SetClip(clip);
		return empty;
    }

    public AudioInstance Play(MonoBehaviour conductor, AudioInstance audioPrefab, float pitch = 1, float volume = 1) {
        return PlayInstance(conductor, audioPrefab, Camera.main.transform,Vector3.zero,pitch,volume);
    }

    public AudioInstance Play(MonoBehaviour conductor, AudioInstance audioPrefab, Transform follow, float pitch = 1, float volume = 1) {
        return PlayInstance(conductor, audioPrefab, follow, Vector3.zero,pitch,volume);
    }

    public AudioInstance Play(MonoBehaviour conductor, AudioInstance audioPrefab, Transform follow, Vector3 offset, float pitch = 1, float volume = 1) {
        return PlayInstance(conductor, audioPrefab, follow, offset,pitch,volume);
    }

    public AudioInstance Play(MonoBehaviour conductor, AudioInstance audioPrefab, Vector3 position, float pitch = 1, float volume = 1) {
        return PlayInstance(conductor, audioPrefab, null, position, pitch, volume);
    }

    AudioInstance PlayInstance(MonoBehaviour conductor, AudioInstance audioPrefab, Transform follow, Vector3 offset, float pitch, float volume) {
        if (audioPrefab == null){
            Debug.LogError("AudioPrefab is null");
            return null;
        }
        AudioInstance ai = PoolMaster.Instance.GetPooledObject(audioPrefab.gameObject).GetComponent<AudioInstance>();
		if (audioPrefab == empty)
			empty.SetClip(null);
        if (audioPrefab.IsSet)
            ai.IsSet = true;
        if (ai == null) {
            Debug.LogError("AudioPrefab doesn't have audioinstance");
            return null;
        }
        ai.Play(conductor,follow, offset, pitch, volume);
        return ai;
    }
}
