using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {

    static GameMaster instance;

    public static GameMaster Instance {
        get {
            if (instance == null) {
                instance = new GameObject("Game Master").AddComponent<GameMaster>();
            }

            return instance;
        }
    }
	
	public bool FinalRound { get; set; }
	public Sprite Background { get; set; }

    int remainingProgress;

    float popSoundFrequencyIncrease = 0.05f;

    public PopSoundType PopSound {
        get; set;
    }

    public int RemainingProgress {
        get {
            return remainingProgress;
        }
        set {
            remainingProgress = value;
            if (maxProgress > 0 && gameCaller != null && RemainingProgress >= 0)
                gameCaller.SetProgress(1f - RemainingProgress / (float)MaxProgress);

        }
    }

    int maxProgress;

    public int MaxProgress {
        get {
            return maxProgress;
        }
        set {
            maxProgress = value;
        }
    }

    int trackedValue;
	AudioClip customClip;

    public int SpaceDust {
        get {
            return trackedValue;
        }
        set {
            trackedValue = value;
            if (gameCaller != null)
                gameCaller.SetTrackedValue(SpaceDust);
        }
    }

	public LevelSettings CurrentLevel { get; set; }
	IGameCaller gameCaller;

	IGameMode currentGameModeHandler;
    Dictionary<GameMode, IGameMode> gameModes = new Dictionary<GameMode, IGameMode>();
	
	public void SetLevel(LevelSettings level) {
		CurrentLevel = level;
	}

	public void SetGameCaller(IGameCaller gameCaller) {
		this.gameCaller = gameCaller;
	}
	
    public void LaunchGame(bool finalRound = false) {
        SetMode(CurrentLevel.gameMode);
		currentGameModeHandler.Initialize(CurrentLevel);
		FinalRound = finalRound;
		gameCaller.LaunchSetup();
		NetworkManager.GetManager().LevelStarted(CurrentLevel.name, false, false);
		StartRound();
    }

    public void RoundDone() {
		gameCaller.RoundDone();
	}

    public void StartRound() {
		if (DebugMaster.Instance.skipPops)
			RoundDone();
		else
			currentGameModeHandler.Activate();
	}

	public void Clicked() {
		gameCaller.Clicked();
	}

	public void ClickDone() {
		gameCaller.ClickDone();
	}

    public void PlayPopSound(int frequency) {
        AudioMaster.Instance.Play(this, GetPopSound(), 1 + Mathf.Log(frequency + 1, 1.5f) * popSoundFrequencyIncrease);
    }

    public AudioInstance GetPopSound() {
        if (customClip == null) {
			return SoundEffectManager.GetManager().GetPopSound();
        }

		return AudioMaster.Instance.InstancifyClip(customClip);
    }

	public void SetCustomClip(AudioClip clip) {
		customClip = clip;
	}

    public void Back() {
		currentGameModeHandler.Back();
    }

    void SetMode(GameMode mode) {
        if (!gameModes.ContainsKey(mode)) {
            switch (mode) {
                case GameMode.Classic:
                    gameModes.Add(mode, new ClassicModeHandler());
                    break;
                case GameMode.Clear:
                    gameModes.Add(mode, new ClearModeHandler());
                    break;
                case GameMode.Regrow:
                    gameModes.Add(mode, new RegrowModeHandler());
                    break;
                case GameMode.Fill:
                    gameModes.Add(mode, new FillModeHandler());
                    break;
                case GameMode.Spot:
                    gameModes.Add(mode, new SpotModeHandler());
                    break;
				case GameMode.Path:
					gameModes.Add(mode, new GameObject("Path Handler").AddComponent<PathGameModeHandler>());
					break;
			}
        }
        currentGameModeHandler = gameModes[mode];
    }

	public float GetDustRatio(float dust) {
		if (!CurrentLevel.spacedustAffectsCoins)
			return 0.5f;
		return Mathf.Clamp((dust - CurrentLevel.spaceDustMin) / (CurrentLevel.spaceDustMax - CurrentLevel.spaceDustMin), 0.1f, 1f); //0.1f min to avoid losing due to dust
	}
}
