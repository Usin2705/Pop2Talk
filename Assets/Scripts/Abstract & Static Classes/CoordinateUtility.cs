using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CoordinateObject {
    public int x;
    public int y;
    public Object myObject;

    public CoordinateObject(int x, int y, Object myObject) {
        this.x = x;
        this.y = y;
        this.myObject = myObject;
    }
}

[System.Serializable]
public struct Coordinate {
    public int x;
    public int y;

    public Coordinate(int x, int y) {
        this.x = x;
        this.y = y;
    }
}

public abstract class CoordinateUtility {

    public static int GetCount(Dictionary<int,Dictionary<int,Tile>> collection) {
        int c = 0;
        if (collection != null) {
            foreach (int i in collection.Keys) {
                c += collection[i].Count;
            }
        }

        return c;
    }

    public static bool ContainsCoordinate(Dictionary<int, Dictionary<int, Tile>> map, int x, int y) {
        if (map == null)
            return false;
        return (map.ContainsKey(x) && map[x].ContainsKey(y));
    }
    
    public static bool ContainsCoordinate(Dictionary<int,System.Collections.Generic.HashSet<int>> set, int x, int y) {
        if (set == null)
            return false;
        return (set.ContainsKey(x) && set[x].Contains(y));
    }
}
