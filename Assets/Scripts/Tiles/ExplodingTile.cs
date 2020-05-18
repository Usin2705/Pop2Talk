using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodingTile : MatchableTile, ISpecialTile {

    bool added;

    public override bool CanPop {
        get {
            return base.CanPop && Moved;
        }
    }

    public bool Moved {get;set;}

    public override void Reset() {
        base.Reset();
        added = false;
        Moved = false;
    }

    public void TrySpecialPop(Coordinate c) {
        if (alreadyPopped)
            return;
        if (!added) {
            GridManager.GetManager().AddPostMoveSpecial(SpecialTileType.Exploding, this);
            if (!Moved)
                added = true;
        } else if (Moved) {
            GridManager.GetManager().AddPostPopSpecial(SpecialTileType.Exploding, c);
            added = false;
        }
    }

    public override void PopVisual(float duration) {
        if (added)
            base.PopVisual(duration);
        else {
            StartCoroutine(ScaleAndVibrate(duration, 1f, 1.5f));
        }
    }
}
