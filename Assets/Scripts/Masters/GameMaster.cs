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

    public int TrackedValue {
        get {
            return trackedValue;
        }
        set {
            trackedValue = value;
            if (gameCaller != null)
                gameCaller.SetTrackedValue(TrackedValue);
        }
    }

	public LevelSettings CurrentLevel { get; set; }
	IGameCaller gameCaller;

	BaseGridGameModeHandler currentGameModeHandler;
    Dictionary<GameMode, BaseGridGameModeHandler> gameModes = new Dictionary<GameMode, BaseGridGameModeHandler>();
	
	public void Initialize(LevelSettings level, IGameCaller gameCaller) {
		CurrentLevel = level;
		this.gameCaller = gameCaller;
	}
	
    public void LaunchGame(bool finalRound = false) {
        SetMode(CurrentLevel.gameType);
		currentGameModeHandler.Initialize(CurrentLevel);
		FinalRound = false;
		gameCaller.LaunchSetup();
        NetworkManager.GetManager().LevelStarted(CurrentLevel.name, LevelSettings.GetDoneStatus(CurrentLevel, NetworkManager.GetManager().Player), LevelSettings.GetMedal(CurrentLevel, NetworkManager.GetManager().Player));
		StartRound();
    }

    public void GameModeDone() {
		gameCaller.RoundDone();
	}

	public void StartRound() {
		if (DebugSettings.Instance.skipPops)
			GameModeDone();
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
                case GameMode.Specific:
                    gameModes.Add(mode, new SpecificModeHandler());
                    break;
                case GameMode.Spot:
                    gameModes.Add(mode, new SpotModeHandler());
                    break;
            }
        }
        currentGameModeHandler = gameModes[mode];
    }

    public void SetSpecificUI(List<MatchType> specificTypes) {
       gameCaller.SetSpecificUI(specificTypes);
    }

	public bool GetMedal() {
		return currentGameModeHandler.GetMedal(TrackedValue, Mathf.FloorToInt(gameCaller.GetMaxRounds() * CurrentLevel.medalValuePerRound));
	}
}
