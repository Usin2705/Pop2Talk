using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegrowModeHandler : BaseGridGameModeHandler {

	int movesPerCard = -1;

    public override void Initialize(LevelSettings level) {
        base.Initialize(level);
    }

    //Tile previousTile;

    public override void TileClicked(Tile t) {
        base.TileClicked(t);
        numberOfPops = 0;
        //previousTile = t;
        List<Dictionary<Tile, Coordinate>> touchingMatches = new List<Dictionary<Tile, Coordinate>>() { GridManager.GetManager().GetTouchingMatches(t) };
        if (!touchingMatches.Contains(null)) {
            GridManager.GetManager().StartCoroutine(StartPopping(t, touchingMatches));
        }
    }

    public override void Activate() {
		base.Activate();
		GameMaster.Instance.MaxProgress = movesPerCard;
		GameMaster.Instance.RemainingProgress = movesPerCard;
	}

    protected override IEnumerator StartPopping(Tile startTile, List<Dictionary<Tile, Coordinate>> touchingMatches) {
        GameMaster.Instance.RemainingProgress--;
        yield return GridManager.GetManager().StartCoroutine(base.StartPopping(startTile, touchingMatches));
        GridPopGrowRecursion(touchingMatches, CheckDone);
    }

    protected override void GridPopGrowRecursion(List<Dictionary<Tile, Coordinate>> touchingMatches, Callback RecursionDone) {
        foreach (Dictionary<Tile,Coordinate> d in touchingMatches)
            GameMaster.Instance.SpaceDust += d.Count;
        base.GridPopGrowRecursion(touchingMatches, RecursionDone);
    }

	protected override void SetupLevelTypes(LevelTypeSettings[] types) {
		SetupGridAndMoves(types, ref movesPerCard);
	}
}
