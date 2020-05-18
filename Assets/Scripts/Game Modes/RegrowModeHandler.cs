using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegrowModeHandler : BaseGridGameModeHandler {

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
        GameMaster.Instance.MaxProgress = level.movesPerCard;
        GameMaster.Instance.RemainingProgress = level.movesPerCard;
		base.Activate();
	}

    protected override IEnumerator StartPopping(Tile startTile, List<Dictionary<Tile, Coordinate>> touchingMatches) {
        GameMaster.Instance.RemainingProgress--;
        yield return GridManager.GetManager().StartCoroutine(base.StartPopping(startTile, touchingMatches));
        GridPopGrowRecursion(touchingMatches, CheckDone);
    }

    protected override void GridPopGrowRecursion(List<Dictionary<Tile, Coordinate>> touchingMatches, Callback RecursionDone) {
        foreach (Dictionary<Tile,Coordinate> d in touchingMatches)
            GameMaster.Instance.TrackedValue += d.Count;
        base.GridPopGrowRecursion(touchingMatches, RecursionDone);
    }

    public override bool GetMedal(int value, int target) {
        return PopMedal(value, target);
    }
}
