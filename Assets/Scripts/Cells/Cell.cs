using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Cell : MonoBehaviour, IPoolable {

    Tile myTile;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite evenSprite;
    [SerializeField] Sprite oddSprite;

    public Tile MyTile {
        get {
            return myTile;
        }

        set {
            if (myTile != null && myTile != value)
                PreviousTile = myTile;
            myTile = value;

        }
    }

    public Tile PreviousTile {
        get;
        protected set;
    }

    public void SetDark(bool dark) {
        float alpha = spriteRenderer.color.a;
        float saturation = (dark) ? 0.7f : 1;
        spriteRenderer.color = new Color(saturation,saturation,saturation,alpha);
    }

    public Dictionary<Vector2, Cell> warps;

    public void SetEven(bool even) {
        if (even) {
            spriteRenderer.sprite = evenSprite;
        } else {
            spriteRenderer.sprite = oddSprite;
        }
    }

    public void OnReturnToPool() {
    }
}
