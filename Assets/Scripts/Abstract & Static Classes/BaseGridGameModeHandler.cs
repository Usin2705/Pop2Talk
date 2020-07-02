using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGridGameModeHandler : IGameMode {

	protected LevelSettings level;
	protected GridSettings gridSettings;
	protected int numberOfPops;
	protected List<Dictionary<Tile, Coordinate>> tilesToGrow;

	protected int clicks;

	public virtual void Initialize(LevelSettings level) {
		this.level = level;
		SetupLevelTypes(this.level.typeSettings);
		GameMaster.Instance.SpaceDust = 0;
		GridManager.GetManager().SetGridSettings(gridSettings);
		GridManager.GetManager().SetGameMode(this);
		GridManager.GetManager().InitializeRandomMatches();
		GridManager.GetManager().CreateGrid();
		GridManager.GetManager().PopulateGrid();
	}

	protected abstract void SetupLevelTypes(LevelTypeSettings[] types);

	protected virtual void SetupGrid(LevelTypeSettings[] types) {
		gridSettings = null;
		foreach (LevelTypeSettings lts in types) {
			if (lts.GetType() == typeof(GridSettings)) {
				gridSettings = (GridSettings)lts;
				break;
			}
		}
		if (gridSettings == null) {
			Debug.LogError("No gridsettings in " + level.name + "!");
		}
	}

	protected virtual void SetupGridAndMoves(LevelTypeSettings[] types, ref int movesPerRound) {
		movesPerRound = -1;
		gridSettings = null;
		IntSettings intSettings;
		foreach (LevelTypeSettings lts in types) {
			if (lts is GridSettings) {
				gridSettings = (GridSettings)lts;
			} else if (lts is IntSettings) {
				intSettings = (IntSettings)lts;
				if (intSettings.integer <= 0) {
					Debug.LogError(intSettings.name + " has round value of " + intSettings.integer);
					return;
				}

				movesPerRound = intSettings.integer;
			}
		}
		if (gridSettings == null) {
			Debug.LogError("No gridsettings in " + level.name + "!");
		}
		if (movesPerRound <= 0) {
			Debug.LogError(level.name + " has no IntSettings with MovesPerRound as the type");
		}
	}


	public virtual void Activate() {
		GridManager.GetManager().IsClickable = true;
		clicks = 0;
	}

	public virtual void FinishedResolvingClick() {

	}

	public virtual void TileClicked(Tile t) {
		clicks++;
		GameMaster.Instance.Clicked();
	}

	protected virtual IEnumerator StartPopping(Tile startTile, List<Dictionary<Tile, Coordinate>> tilesToPop) {
		GridManager.GetManager().IsClickable = false;
		//AudioMaster.Instance.Play(GridManager.GetManager(), SoundEffectManager.GetManager().GetPopSound());
		if (startTile != null) {
			startTile.ClickVisual(GridManager.GetManager().PreclickDuration);
			yield return new WaitForSeconds(GridManager.GetManager().PreclickDuration);
		}
	}

	protected virtual void GridPopDropRecursion(List<Dictionary<Tile, Coordinate>> tilesToPop, Callback RecursionDone, bool createNew = true) {
		GridManager.GetManager().CanMove = true;
		GridManager.GetManager().PopTiles(tilesToPop, () => {
			tilesToPop = GridManager.GetManager().GetPostPopSpecialTilePops();
			if (tilesToPop.Count == 0) {
				GameMaster.Instance.PlayPopSound(numberOfPops);
				numberOfPops++;
				GridManager.GetManager().DropTiles(createNew);
				GridManager.GetManager().MoveTiles(() => {
					tilesToPop = GridManager.GetManager().GetPostMoveSpecialTilePops();
					if (tilesToPop.Count == 0) {
						tilesToPop = GridManager.GetManager().GetAllTouchingMatches(ConstantHolder.minimumCombo);
						if (tilesToPop.Count > 0) {
							GridPopDropRecursion(tilesToPop, RecursionDone, createNew);
						} else
							RecursionDone();
					} else
						GridPopDropRecursion(tilesToPop, RecursionDone, createNew);
				});
			} else {
				GridPopDropRecursion(tilesToPop, RecursionDone, createNew);
			}
		});
	}

	protected virtual void GridPopGrowRecursion(List<Dictionary<Tile, Coordinate>> tilesToPop, Callback RecursionDone) {
		if (tilesToGrow == null)
			tilesToGrow = new List<Dictionary<Tile, Coordinate>>();
		GridManager.GetManager().CanMove = true;
		GridManager.GetManager().PopTiles(tilesToPop, () => {
			GameMaster.Instance.PlayPopSound(numberOfPops);
			numberOfPops++;
			foreach (Dictionary<Tile, Coordinate> d in tilesToPop)
				tilesToGrow.Add(d);
			tilesToPop = GridManager.GetManager().GetPostPopSpecialTilePops();
			if (tilesToPop.Count == 0) {
				GridManager.GetManager().GrowTiles(0.2f, tilesToGrow, () => {
					tilesToGrow.Clear();
					tilesToPop = GridManager.GetManager().GetPostMoveSpecialTilePops();
					if (tilesToPop.Count == 0) {
						tilesToPop = GridManager.GetManager().GetAllTouchingMatches(ConstantHolder.minimumCombo);
						if (tilesToPop.Count > 0) {
							GridPopGrowRecursion(tilesToPop, RecursionDone);
						} else
							RecursionDone();
					} else
						GridPopGrowRecursion(tilesToPop, RecursionDone);
				});
			} else
				GridPopGrowRecursion(tilesToPop, RecursionDone);
		});
	}

	protected virtual void CheckDone() {
		if (GameMaster.Instance.RemainingProgress <= 0) {
			List<Dictionary<Tile, Coordinate>> specialTilesRemaining = GridManager.GetManager().GetRemainingSpecialTiles();
			if (specialTilesRemaining.Count > 0 && GameMaster.Instance.FinalRound) {
				GridManager.GetManager().StartCoroutine(StartPopping(null, specialTilesRemaining));
			} else {
				Done();
			}
		} else {
			GameMaster.Instance.ClickDone();
			GridManager.GetManager().IsClickable = true;
		}
	}

	protected virtual void Done() {
		GameMaster.Instance.RoundDone();
	}

	protected virtual void HandleSpecialTilesAtEnd(Dictionary<Tile, Coordinate> specialTiles) {

	}

	public virtual void Back() {
		GridManager.GetManager().StopGrid();
	}

	public void ClickDustConversion() {
		GameMaster.Instance.SpaceDust += Mathf.RoundToInt(Mathf.Lerp(300, 0, (clicks - 10f) / 15f));
	}
}
