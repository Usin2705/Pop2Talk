using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillModeHandler : BaseGridGameModeHandler {

    public override void Initialize(LevelSettings level) {
        base.Initialize(level);
    }

    Dictionary<Dictionary<Tile, Coordinate>,HashSet<MatchType>> allowed = new Dictionary<Dictionary<Tile, Coordinate>, HashSet<MatchType>>();

    public override void TileClicked(Tile t) {
        base.TileClicked(t);
        numberOfPops = 0;
        List<MatchType> adjacentTypes = new List<MatchType>();
        allowed.Clear();
        foreach (Tile tile in GridManager.GetManager().GetAdjacentTiles(t).Keys) {
            if (tile is MatchableTile && (!(t is MatchableTile) || !TileUtility.TilesMatch(t,tile))) {
                adjacentTypes.Add((tile as MatchableTile).MyMatchType);
            }
        }
		HashSet<MatchType> chosenSet = new HashSet<MatchType>();
        chosenSet.Add((adjacentTypes.Count > 0) ? adjacentTypes.GetRandom() : (t as MatchableTile).MyMatchType);
        List<Dictionary<Tile, Coordinate>> touchingMatches = new List<Dictionary<Tile, Coordinate>>() { GridManager.GetManager().GetTouchingMatches(t) };
        if (!touchingMatches.Contains(null)) {
            foreach (Dictionary<Tile, Coordinate> dic in touchingMatches) {
                allowed.Add(dic, chosenSet);
            }
            GridManager.GetManager().StartCoroutine(StartPopping(t, touchingMatches));
        }
    }

    public override void Activate() {
		base.Activate();
		GameMaster.Instance.MaxProgress = GridManager.GetManager().TileCount;
        GameMaster.Instance.RemainingProgress = GridManager.GetManager().TileCount - GridManager.GetManager().GetLargestMatchingGroupCount();
    }

    protected override IEnumerator StartPopping(Tile startTile, List<Dictionary<Tile, Coordinate>> touchingMatches) {
        yield return GridManager.GetManager().StartCoroutine(base.StartPopping(startTile, touchingMatches));
        GridPopGrow(touchingMatches, CheckDone);
    }

    void GridPopGrow(List<Dictionary<Tile, Coordinate>> touchingMatches, Callback GrowthDone) {
        GridManager.GetManager().CanMove = true;
        GridManager.GetManager().PopTiles(touchingMatches, () => {
            GameMaster.Instance.PlayPopSound(numberOfPops);
            numberOfPops++;
			if (GameMaster.Instance.RemainingProgress > 0) {
				GridManager.GetManager().GrowTiles(0.2f, touchingMatches, allowed, false, () => {
					GrowthDone();
					GameMaster.Instance.RemainingProgress = GridManager.GetManager().CellCount - GridManager.GetManager().GetLargestMatchingGroupCount();
				});
			} else {
				ClickDustConversion();
				GrowthDone();
			}
        });
    }

	protected override void SetupLevelTypes(LevelTypeSettings[] types) {
		SetupGrid(types);
	}
}
