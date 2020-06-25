using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearModeHandler : BaseGridGameModeHandler {
	
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

    public override void Activate() {
		GameMaster.Instance.MaxProgress = GridManager.GetManager().CellCount;
        GridManager.GetManager().DropTiles(true);
        GridManager.GetManager().BreakMatches();
        GameMaster.Instance.RemainingProgress = GridManager.GetManager().TileCount;
        GridManager.GetManager().MoveTiles(base.Activate);
    }

    protected override IEnumerator StartPopping(Tile startTile, List<Dictionary<Tile, Coordinate>> touchingMatches) {
        yield return GridManager.GetManager().StartCoroutine(base.StartPopping(startTile, touchingMatches));
        GridPopDropRecursion(touchingMatches, CheckDone, false);
    }

    protected override void GridPopDropRecursion(List<Dictionary<Tile, Coordinate>> touchingMatches, Callback RecursionDone, bool createNew = true) {
        GameMaster.Instance.RemainingProgress = GridManager.GetManager().TileCount;
        base.GridPopDropRecursion(touchingMatches, RecursionDone, createNew);
    }

    protected override void CheckDone() {
        GameMaster.Instance.RemainingProgress = GridManager.GetManager().TileCount;
        base.CheckDone();
    }

	protected override void Done() {
		ClickDustConversion();
		base.Done();
	}

	protected override void SetupLevelTypes(LevelTypeSettings[] types) {
		SetupGrid(types);
	}
}
