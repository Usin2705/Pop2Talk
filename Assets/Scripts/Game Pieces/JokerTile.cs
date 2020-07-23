using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JokerTile : MatchableTile, ISpecialTile {

    public override void SetMatchType(MatchType type) {
        base.SetMatchType(MatchType.Joker);
    }

    public void TrySpecialPop(Coordinate c) {
        //Doesn't do anything when popped
    }
}
