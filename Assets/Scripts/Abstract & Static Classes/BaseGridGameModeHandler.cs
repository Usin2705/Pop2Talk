using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGridGameModeHandler : IGameMode {

    protected LevelSettings level;
	protected GridSettings gridSettings;
    protected int numberOfPops;
    protected List<Dictionary<Tile,Coordinate>> tilesToGrow;

    public virtual void Initialize(LevelSettings level) {
        this.level = level;
		if (level.levelTypeSettings != null && level.levelTypeSettings is GridSettings) {
			gridSettings = (GridSettings)level.levelTypeSettings;
		} else {
			Debug.LogError("No gridsettings in " + level.name + "!");
		}
        GameMaster.Instance.TrackedValue = 0;
        GridManager.GetManager().SetGridSettings(gridSettings);
        GridManager.GetManager().SetGameMode(this);
        GridManager.GetManager().InitializeRandomMatches();
        GridManager.GetManager().CreateGrid();
        GridManager.GetManager().PopulateGrid();
    }

    public virtual void Activate() {
		GridManager.GetManager().CanClick = true;
    }

    public virtual void FinishedResolvingClick() {

    }

    public virtual void TileClicked(Tile t) {
		GameMaster.Instance.Clicked();
	}

    public virtual bool DoesUseClicks() {
        return true;
    }

    protected virtual IEnumerator StartPopping(Tile startTile, List<Dictionary<Tile, Coordinate>> tilesToPop) {
        GridManager.GetManager().CanClick = false;
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
            foreach(Dictionary<Tile,Coordinate> d in tilesToPop)
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
			if (specialTilesRemaining.Count > 0 && GameMaster.Instance.FinalRound)
				GridManager.GetManager().StartCoroutine(StartPopping(null, specialTilesRemaining));
			else
				GameMaster.Instance.GameModeDone();
		} else {
			GameMaster.Instance.ClickDone();
			GridManager.GetManager().CanClick = true;
		}
    }

    protected virtual void HandleSpecialTilesAtEnd(Dictionary<Tile,Coordinate> specialTiles) {

    }

    public virtual bool GetMedal(int value, int target) {
        return false;
    }

    public virtual bool PopMedal(int value, int target) {
		return target <= value;
    }
    public virtual bool ClickMedal(int value, int target) {
        return target >= value;
    }
    
    public virtual void Back() {
		GridManager.GetManager().StopGrid();
    }
}
