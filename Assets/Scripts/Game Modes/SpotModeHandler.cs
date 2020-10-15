using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotModeHandler : BaseGridGameModeHandler {

	Dictionary<Cell, SpotMarker> spots = new Dictionary<Cell, SpotMarker>();

	int spotCount = -1;

	GameObject spotPrefab;

	public override void Initialize(LevelSettings level) {
		base.Initialize(level);
	}

	public override void TileClicked(Tile t) {
		foreach (Cell c in spots.Keys) {
			if (c.MyTile == t) {
				return;
			}
		}
		base.TileClicked(t);
		numberOfPops = 0;
		List<Dictionary<Tile, Coordinate>> touchingMatches = new List<Dictionary<Tile, Coordinate>>() { GridManager.GetManager().GetTouchingMatches(t) };
		if (!touchingMatches.Contains(null)) {
			GridManager.GetManager().StartCoroutine(StartPopping(t, touchingMatches));
		}
	}

	public override void Activate() {
		base.Activate();
		if (spotPrefab == null)
			spotPrefab = (GameObject)Resources.Load("Spot Marker");
		foreach (Cell c in spots.Keys)
			spots[c].Shrink();
		spots.Clear();
		Cell potentialSpot;
		HashSet<Cell> emptyCells = new HashSet<Cell>();
		while (spots.Count < spotCount) {
			if (spots.Count + emptyCells.Count >= GridManager.GetManager().CellCount)
				break;
			potentialSpot = GridManager.GetManager().GetRandomCell();
			if (emptyCells.Contains(potentialSpot))
				continue;
			if (!spots.ContainsKey(potentialSpot)) {
				if (potentialSpot.MyTile == null) {
					emptyCells.Add(potentialSpot);
					continue;
				}
				spots.Add(potentialSpot, PoolMaster.Instance.GetPooledObject(spotPrefab).GetComponent<SpotMarker>());
				spots[potentialSpot].transform.position = potentialSpot.transform.position;
				spots[potentialSpot].Grow(0.6f);
			}
		}
		GameMaster.Instance.MaxProgress = spotCount;
		GameMaster.Instance.RemainingProgress = spotCount;
	}

	protected override IEnumerator StartPopping(Tile startTile, List<Dictionary<Tile, Coordinate>> touchingMatches) {
		yield return GridManager.GetManager().StartCoroutine(base.StartPopping(startTile, touchingMatches));
		GridPopDropRecursion(touchingMatches, CheckDone);
	}

	protected override void GridPopDropRecursion(List<Dictionary<Tile, Coordinate>> touchingMatches, Callback RecursionDone, bool createNew = true) {
		HashSet<Cell> toRemove = new HashSet<Cell>();
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
				GameMaster.Instance.RemainingProgress--;
			}
		}
		foreach (Cell c in toRemove)
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
	protected override void Done() {
		ClickDustConversion();
		base.Done();
	}

	protected override void SetupLevelTypes(LevelTypeSettings[] types) {
		SetupGrid(types);
		spotCount = -1;
		IntSettings intSettings;
		foreach (LevelTypeSettings lts in types) {
			if (lts is IntSettings) {
				intSettings = (IntSettings)lts;
				if (intSettings.integer <= 0) {
					Debug.LogError(intSettings.name + " has int value of " + intSettings.integer);
					return;
				}

				spotCount = intSettings.integer;
				break;
			}
		}
		if (spotCount <= 0) {
			Debug.LogError(level.name + " has no IntSettings with Other as the type");
		}
	}
}
