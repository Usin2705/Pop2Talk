using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Grid Settings", menuName = "CustomObjects/Grid Settings")]
public class GridSettings : LevelTypeSettings {

	public int gridWidth;
	public int gridHeight;
	public Vector3 gridCenter;
	public int matchTypes = 4;
	public float cellSize;
	[Space]
	public int randomHoles;
	public Coordinate[] specificHoles;
	public Tile[] specialTiles;
}
