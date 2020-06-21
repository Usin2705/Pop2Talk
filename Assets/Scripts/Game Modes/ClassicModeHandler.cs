using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassicModeHandler : BaseGridGameModeHandler {

	int movesPerRound = -1;

    public override void Initialize(LevelSettings level) {
        base.Initialize(level);
    }

    public override void TileClicked(Tile t) {
        base.TileClicked(t);
        numberOfPops = 0;
        List<Dictionary<Tile, Coordinate>> touchingMatches = new List<Dictionary<Tile, Coordinate>>() { GridManager.GetManager().GetTouchingMatches(t) };
        if (!touchingMatches.Contains(null)) {
            GridManager.GetManager().StartCoroutine(StartPopping(t, touchingMatches));
        }
    }

	protected override void SetupLevelTypes(LevelTypeSettings[] types) {
		SetupGridAndMoves(types, ref movesPerRound);
	}

	public override void Activate() {
        GridGameMaster.Instance.MaxProgress = movesPerRound;
        GridGameMaster.Instance.RemainingProgress = movesPerRound;
		base.Activate();
	}

    protected override IEnumerator StartPopping(Tile startTile, List<Dictionary<Tile, Coordinate>> touchingMatches) {
        GridGameMaster.Instance.RemainingProgress--;
        yield return GridManager.GetManager().StartCoroutine(base.StartPopping(startTile, touchingMatches));
        GridPopDropRecursion(touchingMatches, CheckDone);
    }

    protected override void GridPopDropRecursion(List<Dictionary<Tile, Coordinate>> touchingMatches, Callback RecursionDone, bool createNew = true) {
        foreach (Dictionary<Tile,Coordinate> d in touchingMatches)
            GridGameMaster.Instance.SpaceDust += d.Count;
        base.GridPopDropRecursion(touchingMatches, RecursionDone, createNew);
    }
}
