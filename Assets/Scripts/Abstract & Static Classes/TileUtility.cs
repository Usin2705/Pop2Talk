using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileUtility {

	public static bool TilesMatch(Tile a, Tile b) {
        if (a == null || !(a is MatchableTile) || b == null || !(b is MatchableTile))
            return false;
        return ((a as MatchableTile).MyMatchType == (b as MatchableTile).MyMatchType || (a as MatchableTile).MyMatchType == MatchType.Joker || (b as MatchableTile).MyMatchType == MatchType.Joker);
    }
}
