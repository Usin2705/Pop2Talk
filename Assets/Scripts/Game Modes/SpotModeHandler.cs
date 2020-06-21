using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotModeHandler : BaseGridGameModeHandler {
    
    Dictionary<Cell, SpotMarker> spots = new Dictionary<Cell, SpotMarker>();

    int spotCount = 15;

    GameObject spotPrefab;

    public override void Initialize(LevelSettings level) {
        base.Initialize(level);
    }

    public override void TileClicked(Tile t) {
        base.TileClicked(t);
        numberOfPops = 0;
        GridGameMaster.Instance.SpaceDust++;
        List<Dictionary<Tile, Coordinate>> touchingMatches = new List<Dictionary<Tile, Coordinate>>() { GridManager.GetManager().GetTouchingMatches(t) };
        if (!touchingMatches.Contains(null)) {
            GridManager.GetManager().StartCoroutine(StartPopping(t, touchingMatches));
        }
    }

    public override void Activate() {
        if (spotPrefab == null)
            spotPrefab = (GameObject)Resources.Load("Spot Marker");
        foreach (Cell c in spots.Keys)
            spots[c].Shrink();
        spots.Clear();
        Cell potentialSpot;
        while (spots.Count < spotCount) {
            if (spots.Count >= GridManager.GetManager().CellCount)
                break;
            potentialSpot = GridManager.GetManager().GetRandomCell();
            if (!spots.ContainsKey(potentialSpot)) {
                spots.Add(potentialSpot, PoolMaster.Instance.GetPooledObject(spotPrefab).GetComponent<SpotMarker>());
                spots[potentialSpot].transform.position = potentialSpot.transform.position;
                spots[potentialSpot].Grow(() => { });
            }
        }
        GridGameMaster.Instance.MaxProgress = spotCount;
        GridGameMaster.Instance.RemainingProgress = spotCount;
		base.Activate();
	}

    protected override IEnumerator StartPopping(Tile startTile, List<Dictionary<Tile, Coordinate>> touchingMatches) {
        yield return GridManager.GetManager().StartCoroutine(base.StartPopping(startTile, touchingMatches));
        GridPopDropRecursion(touchingMatches, CheckDone);
    }

    protected override void GridPopDropRecursion(List<Dictionary<Tile, Coordinate>> touchingMatches, Callback RecursionDone, bool createNew = true) {
        System.Collections.Generic.HashSet<Cell> toRemove = new System.Collections.Generic.HashSet<Cell>();
        foreach (Dictionary<Tile, Coordinate> d in touchingMatches) {
            Cell popped = null;
            foreach (Cell c in spots.Keys) {
                popped = null;
                foreach (Tile t in d.Keys) {
                    if (c.MyTile == t) {
                        popped = c;
                        break;
                    }
                }
                if (popped == null)
                    continue;
                spots[popped].Shrink();
                toRemove.Add(popped);
                GridGameMaster.Instance.RemainingProgress--;
            }
        }
        foreach(Cell c in toRemove)
            spots.Remove(c);
        base.GridPopDropRecursion(touchingMatches, RecursionDone, createNew);
    }
    
    public override void Back() {
        foreach (Cell c in spots.Keys) {
            PoolMaster.Instance.Destroy(spots[c].gameObject);
        }
        spots.Clear();
        base.Back();
    }

	protected override void SetupLevelTypes(LevelTypeSettings[] types) {
		SetupGrid(types);
	}
}
