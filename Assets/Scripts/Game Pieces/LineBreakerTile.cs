using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineBreakerTile : MatchableTile, ISpecialTile {

    [SerializeField] bool horizontal = false;

    public void TrySpecialPop(Coordinate c) {
        if (alreadyPopped)
            return;
        GridManager.GetManager().AddPostPopSpecial((horizontal) ? SpecialTileType.Horizontal : SpecialTileType.Vertical, c);
    }
}
