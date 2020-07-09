using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniqueIdentifierAttribute : PropertyAttribute { }

public enum GameMode { Classic, Clear, Fill, Spot, Regrow, Path, Hide, Speed };

public enum WordCardType { Repeat, Memory };

public enum PopSoundType { Sound, Next, Random };

public enum Direction { Down, Up, Left, Right };

public enum SpecialTileType { None, Exploding, Horizontal, Vertical, Joker};

public enum Language { None, Finnish, EnglishUK };

public enum CosmeticSlot {Ship, Wallpaper };

public delegate void Callback();
public delegate void IntCallback(int i);

public abstract class ConstantHolder : MonoBehaviour {
    

    public static readonly Dictionary<MatchType, Color> placeHolderGraphics = new Dictionary<MatchType, Color>() { { MatchType.Red, Color.red}, { MatchType.Blue, Color.blue},
        { MatchType.Green, Color.green}, { MatchType.Yellow, Color.yellow},{ MatchType.Orange, Color.cyan},{ MatchType.Purple, Color.magenta}, {MatchType.Joker, Color.black }};

    public static readonly int minimumCombo = 4;
    public static readonly float gravity = 25f;
    public static readonly int numberOfTypes = 6;
    public static readonly int minimumTypes = 4;
    public static readonly int specialTileThreshold = 5;
    public static readonly int interactableLayer = LayerMask.GetMask("Interactable");
    public static readonly int maxStars = 5;
    //public static readonly Dictionary<Direction, Vector2> directionToVector = new Dictionary<Direction, Vector2>() { { Direction.None, Vector3.zero }, { Direction.Up, Vector2.up }, { Direction.Down, Vector2.down }, { Direction.Left, Vector2.left }, { Direction.Right, Vector2.right } };
    //public static readonly Dictionary<Vector2, Direction> vectorToDirection = new Dictionary<Vector2, Direction>() { { Vector3.zero, Direction.None }, { Vector2.up, Direction.Up }, { Vector2.down, Direction.Down }, { Direction.Left, Vector2.left }, { Direction.Right, Vector2.right } };
}
