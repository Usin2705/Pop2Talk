using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecificModeHandler : BaseGridGameModeHandler {

    int thisRoundTrack;
    bool canTrack;
    List<MatchType> specifiedTypes = new List<MatchType>();
    bool poppedSpecified;

    int specifiedCount = 3;

    public override void Initialize(LevelSettings level) {
        base.Initialize(level);
    }

    public override void TileClicked(Tile t) {
        base.TileClicked(t);
        numberOfPops = 0;
        poppedSpecified = false;
        List<Dictionary<Tile, Coordinate>> touchingMatches = new List<Dictionary<Tile, Coordinate>>() { GridManager.GetManager().GetTouchingMatches(t) };
        if (!touchingMatches.Contains(null)) {
            GridManager.GetManager().StartCoroutine(StartPopping(t, touchingMatches));
        }
    }

    public override void Activate() {
        thisRoundTrack = 0;
        canTrack = true;
        specifiedTypes.Clear();
        for (int i = 0; i < specifiedCount; ++i) {
            specifiedTypes.Add((MatchType)(Random.Range(0, gridSettings.matchTypes)+1));
        }
        GameMaster.Instance.SetSpecificUI(specifiedTypes);
        GameMaster.Instance.MaxProgress = specifiedCount;
        GameMaster.Instance.RemainingProgress = specifiedCount;
		base.Activate();
	}

    protected override IEnumerator StartPopping(Tile startTile, List<Dictionary<Tile, Coordinate>> touchingMatches) {
        yield return GridManager.GetManager().StartCoroutine(base.StartPopping(startTile, touchingMatches));
        GridPopDropRecursion(touchingMatches, CheckDone);
    }

    protected override void GridPopDropRecursion(List<Dictionary<Tile, Coordinate>> touchingMatches, Callback RecursionDone, bool createNew = true) {
        System.Collections.Generic.HashSet<MatchType> foundTypes = new System.Collections.Generic.HashSet<MatchType>();
        MatchableTile mt;
        foreach (Dictionary<Tile, Coordinate> d in touchingMatches) {
            if (canTrack) {
                GameMaster.Instance.TrackedValue += d.Count;
                thisRoundTrack += d.Count;
            }
            foundTypes.Clear();
            if (!poppedSpecified) {
                foreach (Tile t in d.Keys) {
                    if (t is MatchableTile)
                        mt = (t as MatchableTile);
                    else
                        continue;
                    if (!foundTypes.Contains(mt.MyMatchType) && specifiedTypes.Contains(mt.MyMatchType) && mt.MyMatchType != MatchType.None) {
                        for (int i = 0; i < specifiedTypes.Count; ++i) {
                            if (specifiedTypes[i] == mt.MyMatchType || mt.MyMatchType == MatchType.Joker) {
                                specifiedTypes[i] = MatchType.None;
                                break;
                            }
                        }
                        foundTypes.Add(mt.MyMatchType);
                        poppedSpecified = true;
                        GameMaster.Instance.RemainingProgress--;
                        break;
                    }
                }
            }
        }
        GameMaster.Instance.SetSpecificUI(specifiedTypes);
        base.GridPopDropRecursion(touchingMatches, RecursionDone, createNew);
    }

    protected override void CheckDone() {
        if (!poppedSpecified && canTrack) {
            canTrack = false;
            GameMaster.Instance.TrackedValue -= thisRoundTrack;
        }
        base.CheckDone();
    }

    public override bool GetMedal(int value, int target) {
        return PopMedal(value, target);
    }
}
