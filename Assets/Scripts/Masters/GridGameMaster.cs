using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGameMaster : MonoBehaviour {

    static GridGameMaster instance;

    public static GridGameMaster Instance {
        get {
            if (instance == null) {
                instance = new GameObject("Grid Game Master").AddComponent<GridGameMaster>();
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

	public GridLevelSettings CurrentLevel { get; set; }
	IGameCaller gameCaller;

	BaseGridGameModeHandler currentGameModeHandler;
    Dictionary<GridGameMode, BaseGridGameModeHandler> gameModes = new Dictionary<GridGameMode, BaseGridGameModeHandler>();
	
	public void SetLevel(GridLevelSettings level) {
		CurrentLevel = level;
	}

	public void SetGameCaller(IGameCaller gameCaller) {
		this.gameCaller = gameCaller;
	}
	
    public void LaunchGame(bool finalRound = false) {
        SetMode(CurrentLevel.gameMode);
		currentGameModeHandler.Initialize(CurrentLevel);
		FinalRound = false;
		gameCaller.LaunchSetup();
		NetworkManager.GetManager().LevelStarted(CurrentLevel.name, false, false);
		StartRound();
    }

    public void RoundDone() {
		gameCaller.RoundDone();
	}

    public void StartRound() {
		if (DebugSettings.Instance.skipPops)
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

    void SetMode(GridGameMode mode) {
        if (!gameModes.ContainsKey(mode)) {
            switch (mode) {
                case GridGameMode.Classic:
                    gameModes.Add(mode, new ClassicModeHandler());
                    break;
                case GridGameMode.Clear:
                    gameModes.Add(mode, new ClearModeHandler());
                    break;
                case GridGameMode.Regrow:
                    gameModes.Add(mode, new RegrowModeHandler());
                    break;
                case GridGameMode.Fill:
                    gameModes.Add(mode, new FillModeHandler());
                    break;
                case GridGameMode.Spot:
                    gameModes.Add(mode, new SpotModeHandler());
                    break;
            }
        }
        currentGameModeHandler = gameModes[mode];
    }

	public float GetDustRatio(float dust) {
		if (!CurrentLevel.spacedustAffectsCoins)
			return 0.5f;
		return Mathf.Clamp01((dust - CurrentLevel.spaceDustMin) / (CurrentLevel.spaceDustMax - CurrentLevel.spaceDustMin));
	}
}
