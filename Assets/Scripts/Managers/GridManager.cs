using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GridManager : MonoBehaviour, ITileClickReceiver {

	[SerializeField] float maxFallSpeed;
	[SerializeField] float jumpSpeed;
	[SerializeField] float popVisualDuration;
	[SerializeField] float preclickDuration = 0.4f;
	[Space]
	[SerializeField] Cell cellPrefab;
	[SerializeField] MatchableTile matchableTilePrefab;

	[SerializeField] MatchTypeSpriteDictionary matchSprites;

	GridSettings gridSettings;
	BaseGridGameModeHandler gridGameMode;

	static GridManager gridManager;
	Transform gridRoot;
	Transform cellRoot;
	Transform tileRoot;

	Cell[][] grid;
	Dictionary<Tile, TileMotion> tileMotions = new Dictionary<Tile, TileMotion>();

	Dictionary<SpecialTileType, HashSet<Coordinate>> postPopSpecialTiles = new Dictionary<SpecialTileType, HashSet<Coordinate>>();
	Dictionary<SpecialTileType, HashSet<Tile>> postMoveSpecialTiles = new Dictionary<SpecialTileType, HashSet<Tile>>();

	int diagonalDirection = 1;
	float speed;

	public float PreclickDuration {
		get {
			return preclickDuration;
		}

		private set {
			preclickDuration = value;
		}
	}

	public bool Moving { get; protected set; }

	bool isClickable;
	Dictionary<int, int> lowestPops = new Dictionary<int, int>();

	class TileMotion {
		Queue<Cell> cellsToMoveThrough = new Queue<Cell>();
		Queue<Coordinate> coordinatesToMoveThrough = new Queue<Coordinate>();

		public int startX = -1;
		public int startY = -1;

		public int lastX = -1;
		public int lastY = -1;

		public Cell PrevCell { get; set; }
		public Coordinate PrevCoordinate { get; set; }

		public void Enqueue(Cell cell, Coordinate coordinate) {
			cellsToMoveThrough.Enqueue(cell);
			coordinatesToMoveThrough.Enqueue(coordinate);
		}

		public Cell PeekCell() {
			return cellsToMoveThrough.Peek();
		}

		public Coordinate PeekCoordinate() {
			return coordinatesToMoveThrough.Peek();
		}

		public Cell Dequeue() {
			PrevCell = cellsToMoveThrough.Dequeue();
			PrevCoordinate = coordinatesToMoveThrough.Dequeue();
			return PrevCell;
		}

		public int Count {
			get {
				return cellsToMoveThrough.Count;
			}
		}
	}

	class TilemotionComparer : IComparer<TileMotion> {
		public int Compare(TileMotion t1, TileMotion t2) {
			if (t1.startX == t1.lastX && t2.startX != t1.startX)
				return -1;
			if (t1.startX != t1.lastX && t2.startX == t1.startX)
				return 1;

			int result = t1.startY.CompareTo(t2.startY);
			if (result == 0) {
				result = t1.lastY.CompareTo(t2.lastY);
				if (result == 0)
					return -1;
			}
			return result;
		}
	}


	public bool IsClickable {
		get {
			return isClickable;
		}

		set {
			isClickable = value;
			if (grid != null) {
				foreach (Cell[] array in grid) {
					foreach (Cell c in array) {
						if (c == null)
							continue;
						c.SetDark(!isClickable);
					}
				}
			}
		}
	}

	public bool CanClick() {
		return IsClickable && !Moving;
	}

	public bool CanMove { get; set; }

	public int TileCount { get; private set; }

	public int CellCount { get; private set; }

	bool forceDownGravity;

	public Direction Gravity { get; set; } = Direction.Down;

	public bool CreateSpecialTiles {
		get {
			return (gridSettings != null && gridSettings.specialTiles.Length > 0);
		}
	}

	public Tile GetRandomSpecialTileToCreate() {
		return (gridSettings == null) ? null : gridSettings.specialTiles.GetRandom();
	}

	bool tetrisRandom = true;

	List<MatchType> availableRandomMatchTypes = new List<MatchType>();
	int randomIndex;

	MatchType GetRandomMatchType(HashSet<MatchType> matchTypes, bool typesAreExcluded) {
		if (matchTypes != null && matchTypes.Count == gridSettings.matchTypes) {
			Debug.LogError("Random match type getting has exclusions equal to the number of available matchtypes");
			return MatchType.None;
		}
		if (tetrisRandom) {
			MatchType m = MatchType.None;
			for (int i = randomIndex; i < availableRandomMatchTypes.Count; ++i) {
				if (matchTypes == null || matchTypes.Count == 0 || matchTypes.Contains(availableRandomMatchTypes[i]) != typesAreExcluded) {
					m = availableRandomMatchTypes[i];
					if (i > randomIndex) {
						availableRandomMatchTypes[i] = availableRandomMatchTypes[randomIndex];
						availableRandomMatchTypes[randomIndex] = m;
					}
					if (i >= randomIndex)
						randomIndex++;
					break;
				}
				if (i == availableRandomMatchTypes.Count - 1 && m == MatchType.None) {
					availableRandomMatchTypes.Shuffle(randomIndex);
					i = -1;
				}
			}
			if (randomIndex == availableRandomMatchTypes.Count) {
				availableRandomMatchTypes.Shuffle();
				randomIndex = 0;
			}
			return m;
		} else {
			int maxTypes = Mathf.Clamp(gridSettings.matchTypes, ConstantHolder.minimumTypes, ConstantHolder.numberOfTypes);
			MatchType type;
			if (matchTypes != null && maxTypes <= matchTypes.Count) {
				Debug.LogError("Excluded matchtypes outnumber available types");
				return MatchType.None;
			}
			do {
				type = (MatchType)Random.Range(0, maxTypes) + 1;
			} while (type == MatchType.None || (matchTypes != null && matchTypes.Contains(type)));
			return type;
		}
	}

	public void InitializeRandomMatches() {
		availableRandomMatchTypes = new List<MatchType>();
		for (int i = 0; i < gridSettings.matchTypes; ++i) {
			availableRandomMatchTypes.Add((MatchType)(i + 1));
		}
		availableRandomMatchTypes.Shuffle();
		randomIndex = 0;
	}

	public void AddPostPopSpecial(SpecialTileType st, Coordinate c) {
		if (!postPopSpecialTiles.ContainsKey(st))
			postPopSpecialTiles.Add(st, new HashSet<Coordinate>());
		postPopSpecialTiles[st].Add(c);
	}


	public void AddPostMoveSpecial(SpecialTileType st, Tile t) {
		if (!postMoveSpecialTiles.ContainsKey(st))
			postMoveSpecialTiles.Add(st, new HashSet<Tile>());
		postMoveSpecialTiles[st].Add(t);
	}

	void Awake() {
		if (gridManager != null) {
			Debug.LogError("Multiple GridManagers");
			Destroy(gameObject);
			return;
		}
		gridManager = this;
	}

	public static GridManager GetManager() {
		return gridManager;
	}

	public void SetGridSettings(GridSettings gridSettings) {
		this.gridSettings = gridSettings;
	}

	public void SetGameMode(BaseGridGameModeHandler gridGameMode) {
		this.gridGameMode = gridGameMode;
	}

	Dictionary<int, HashSet<int>> GetHoles() {
		if (gridSettings.specificHoles.Length == 0 && gridSettings.randomHoles == 0)
			return null;
		Dictionary<int, HashSet<int>> holes = new Dictionary<int, HashSet<int>>();
		HashSet<int> indices = new HashSet<int>();
		int edgeHoles = 0;
		foreach (Coordinate c in gridSettings.specificHoles) {
			if (c.x == 0 || c.x == gridSettings.gridWidth - 1 || c.y == 0 || c.y == gridSettings.gridHeight - 1) {
				if (!holes.ContainsKey(c.x))
					holes.Add(c.x, new HashSet<int>());
				if (!holes[c.x].Contains(c.y)) {
					holes[c.x].Add(c.y);
					edgeHoles++;
				}
			} else {
				if (!indices.Contains((c.x - 1) * (gridSettings.gridHeight - 2) + c.y - 1)) {
					indices.Add((c.x - 1) * (gridSettings.gridHeight - 2) + c.y - 1);
				}
			}
		}
		while (indices.Count < gridSettings.randomHoles + gridSettings.specificHoles.Length - edgeHoles && indices.Count < (gridSettings.gridHeight - 2) * (gridSettings.gridWidth - 2)) {
			int index = Random.Range(0, (gridSettings.gridHeight - 2) * (gridSettings.gridWidth - 2));
			if (!indices.Contains(index))
				indices.Add(index);
		}
		foreach (int i in indices) {
			int x = Mathf.FloorToInt(i / (gridSettings.gridHeight - 2f)) + 1;
			if (!holes.ContainsKey(x))
				holes.Add(x, new HashSet<int>());
			holes[x].Add(i - (x - 1) * (gridSettings.gridHeight - 2) + 1);
		}
		return holes;
	}

	public void CreateGrid() {
		Dictionary<int, HashSet<int>> acellularCoordinates = GetHoles();
		if (gridSettings == null) {
			Debug.LogError("No level settings");
			return;
		}
		if (cellPrefab == null) {
			Debug.LogError("Cell prefab is null. Stopping creating the grid.");
			return;
		}
		if (gridRoot != null) {
			ClearGrid();
		} else {
			gridRoot = new GameObject().transform;
			gridRoot.name = "Grid";
		}
		if (cellRoot == null) {
			cellRoot = new GameObject().transform;
			cellRoot.name = "Cell Root";
			cellRoot.SetParent(gridRoot);
		}
		grid = new Cell[gridSettings.gridWidth][];
		CellCount = 0;
		forceDownGravity = true;
		bool hasExclusions = (acellularCoordinates != null && acellularCoordinates.Count > 0);
		for (int x = 0; x < GetGridWidth(); ++x) {
			grid[x] = new Cell[gridSettings.gridHeight];
			for (int y = 0; y < GetGridHeight(); ++y) {
				if (!hasExclusions || !(CoordinateUtility.ContainsCoordinate(acellularCoordinates, x, y))) {
					Cell c = PoolMaster.Instance.GetPooledObject(cellPrefab.gameObject).GetComponent<Cell>();
					grid[x][y] = c;
					c.transform.SetParent(cellRoot);
					c.transform.position = gridSettings.gridCenter + new Vector3((-gridSettings.gridWidth / 2f + x + 0.5f) * gridSettings.cellSize, (-gridSettings.gridHeight / 2f + y + 0.5f) * gridSettings.cellSize);
					c.transform.localScale = Vector3.one * gridSettings.cellSize;
					c.name = "Cell " + (x + 1) + " " + (y + 1);
					c.SetEven((x + y) % 2 == 0);
					CellCount++;
				}
			}
		}
		forceDownGravity = false;
	}

	public void PopulateGrid(List<Tile> specialTiles = null, Dictionary<int, Dictionary<int, Tile>> specificTiles = null) {
		if (grid == null) {
			Debug.LogError("Grid is null. Stopping populating the grid.");
			return;
		}
		if (tileRoot == null) {
			tileRoot = new GameObject().transform;
			tileRoot.name = "Tile Root";
			tileRoot.SetParent(gridRoot);
		}

		/*List<Tile> tilesToAdd = (specialTiles != null) ? new List<Tile>(specialTiles) : new List<Tile>();
        int nonRandomTileCount = CoordinateUtility.GetCount(specificTiles) + ((specialTiles == null) ? 0 : specialTiles.Count);

        if (nonRandomTileCount < cellCount) {
            if (matchableTilePrefab == null) {
                Debug.LogError("Matchable tile prefab is null. Stopping populating the grid");
                return;
            }
            for (int i = 0; i < cellCount - nonRandomTileCount; ++i) {
                tilesToAdd.Add(matchableTilePrefab);
            }
        }

        tilesToAdd.Shuffle();
        Tile t;
        int tileToAddIndex = 0;
		for (int y = 0; y < GetGridHeight(); ++y) {
			for (int x = 0; x < GetGridWidth(); ++x) {
				if (GetCell(x, y) != null) {
					if (CoordinateUtility.ContainsCoordinate(specificTiles, x, y)) {
						t = specificTiles[x][y];
					} else {
						t = tilesToAdd[tileToAddIndex];
						tileToAddIndex++;
					}
					InstantiateTile(t, x, GetGridHeight() - 1, true);
				}
			}
		}*/
		DropTiles(true);
		BreakMatches();
		CanMove = true;
		MoveTiles(() => { CanMove = false; });
	}

	Tile InstantiateTile(Tile t, int x, int y, bool toTheTop = false, HashSet<MatchType> matchTypes = null, bool typesAreExcluded = true) {
		return InstantiateTile(t, GetCell(x, y), toTheTop, matchTypes, typesAreExcluded);
	}

	Tile InstantiateTile(Tile t, Cell c, bool toTheTop = false, HashSet<MatchType> matchTypes = null, bool typesAreExcluded = true) {
		if (t != null) {
			t = PoolMaster.Instance.GetPooledObject(t.gameObject).GetComponent<Tile>();
			t.Reset();
			t.Receiver = this;
			c.MyTile = t;
			t.transform.position = (toTheTop) ? c.transform.position - GetGravityVector() * gridSettings.cellSize : c.transform.position;
			t.SetScale (Vector3.one * gridSettings.cellSize);
			t.transform.SetParent(tileRoot);
			if (t is MatchableTile) {
				(t as MatchableTile).SetMatchType(GetRandomMatchType(matchTypes, typesAreExcluded));
				(t as MatchableTile).SetStencil(true);
			}
			TileCount++;
			return t;
		} else {
			Debug.LogError("Null tile-prefab in instantiation");
		}
		return null;
	}

	public void BreakMatches() {
		int change = 0;
		HashSet<MatchType> adjacentTypes = new HashSet<MatchType>();
		Cell c;
		for (int x = 0; x < GetGridWidth(); ++x) {
			for (int y = 0; y < GetGridHeight(); ++y) {
				c = GetCell(x, y);
				if (c != null && c.MyTile != null && c.MyTile is MatchableTile) {
					change = 0;
					adjacentTypes.Clear();
					if (CheckSameMatchTypeAndAddTypeToSet(c.MyTile, x - 1, y, adjacentTypes))
						change++;
					if (CheckSameMatchTypeAndAddTypeToSet(c.MyTile, x + 1, y, adjacentTypes))
						change++;
					if (CheckSameMatchTypeAndAddTypeToSet(c.MyTile, x, y - 1, adjacentTypes))
						change++;
					if (CheckSameMatchTypeAndAddTypeToSet(c.MyTile, x, y + 1, adjacentTypes))
						change++;

					if (change > 1) {
						(c.MyTile as MatchableTile).SetRandomMatchType(gridSettings.matchTypes, adjacentTypes);
					}
				}
			}
		}
	}

	bool CheckSameMatchTypeAndAddTypeToSet(Tile t, int x, int y, HashSet<MatchType> set, bool addNonMatching = false) {
		if (x >= 0 && x < GetGridWidth() && y >= 0 && y < GetGridHeight() && GetCell(x, y) != null && GetCell(x, y).MyTile is MatchableTile && ((GetCell(x, y).MyTile as MatchableTile).MyMatchType != MatchType.Joker)) {
			if (!set.Contains((GetCell(x, y).MyTile as MatchableTile).MyMatchType))
				set.Add((GetCell(x, y).MyTile as MatchableTile).MyMatchType);
			return (TileUtility.TilesMatch(t, GetCell(x, y).MyTile));
		}
		return false;
	}

	public void ClickTile(Tile t) {
		if (!IsClickable)
			return;
		gridGameMode.TileClicked(t);
	}

	public Dictionary<Tile, Coordinate> GetTouchingMatches(Tile t) {
		for (int x = 0; x < GetGridWidth(); ++x) {
			for (int y = 0; y < GetGridHeight(); ++y) {
				if (GetCell(x, y) != null && GetCell(x, y).MyTile == t) {
					return GetTouchingMatches(x, y);
				}
			}
		}
		return null;
	}

	public List<Dictionary<Tile, Coordinate>> GetRemainingSpecialTiles() {
		List<Dictionary<Tile, Coordinate>> remainingSpecials = new List<Dictionary<Tile, Coordinate>>();
		for (int x = 0; x < GetGridWidth(); ++x) {
			for (int y = 0; y < GetGridHeight(); ++y) {
				if (GetCell(x, y) != null && GetCell(x, y).MyTile is ISpecialTile) {
					if (GetCell(x, y).MyTile is JokerTile)
						remainingSpecials.Add(GetTouchingMatches(x, y));
					else
						remainingSpecials.Add(new Dictionary<Tile, Coordinate>() { { GetCell(x, y).MyTile, new Coordinate(x, y) } });
				}
			}
		}
		return remainingSpecials;
	}

	public Dictionary<Tile, Coordinate> GetTouchingMatches(int x, int y, Dictionary<Tile, Coordinate> adjacents = null) {
		if (adjacents == null)
			adjacents = new Dictionary<Tile, Coordinate>();
		Queue<Coordinate> search = new Queue<Coordinate>();
		if (!adjacents.ContainsKey(GetCell(x, y).MyTile))
			search.Enqueue(new Coordinate(x, y));
		else
			return adjacents;
		Coordinate c;
		MatchType groupType = (GetCell(x, y).MyTile as MatchableTile).MyMatchType;
		if (groupType == MatchType.Joker) {
			for (int i = 0; i < 4; ++i) {
				switch (i) {
					case 0:
						if (x != 0) {
							c = new Coordinate(x - 1, y);
						} else
							continue;
						break;
					case 1:
						if (x != GetGridWidth() - 1) {
							c = new Coordinate(x + 1, y);
						} else
							continue;
						break;
					case 2:
						if (y != 0) {
							c = new Coordinate(x, y - 1);
						} else
							continue;
						break;
					default:
						if (y != GetGridHeight() - 1) {
							c = new Coordinate(x, y + 1);
						} else
							continue;
						break;
				}
				GetTouchingMatches(c.x, c.y, adjacents);
			}
			return adjacents;
		}
		while (search.Count > 0) {
			c = search.Dequeue();
			if (!adjacents.ContainsKey(GetCell(c.x, c.y).MyTile)) {
				adjacents.Add(GetCell(c.x, c.y).MyTile, c);
			} else
				continue;
			if (c.x != 0 && GetCell(c.x - 1, c.y) != null && TileUtility.TilesMatch(GetCell(c.x, c.y).MyTile, GetCell(c.x - 1, c.y).MyTile) && !adjacents.ContainsKey(GetCell(c.x - 1, c.y).MyTile)) {
				if ((GetCell(c.x - 1, c.y).MyTile as MatchableTile).MyMatchType == groupType || (GetCell(c.x - 1, c.y).MyTile as MatchableTile).MyMatchType == MatchType.Joker)
					search.Enqueue(new Coordinate(c.x - 1, c.y));
			}
			if (c.x != GetGridWidth() - 1 && GetCell(c.x + 1, c.y) != null && TileUtility.TilesMatch(GetCell(c.x, c.y).MyTile, GetCell(c.x + 1, c.y).MyTile) && !adjacents.ContainsKey(GetCell(c.x + 1, c.y).MyTile)) {
				if ((GetCell(c.x + 1, c.y).MyTile as MatchableTile).MyMatchType == groupType || (GetCell(c.x + 1, c.y).MyTile as MatchableTile).MyMatchType == MatchType.Joker)
					search.Enqueue(new Coordinate(c.x + 1, c.y));
			}
			if (c.y != 0 && GetCell(c.x, c.y - 1) != null && TileUtility.TilesMatch(GetCell(c.x, c.y).MyTile, GetCell(c.x, c.y - 1).MyTile) && !adjacents.ContainsKey(GetCell(c.x, c.y - 1).MyTile)) {
				if ((GetCell(c.x, c.y - 1).MyTile as MatchableTile).MyMatchType == groupType || (GetCell(c.x, c.y - 1).MyTile as MatchableTile).MyMatchType == MatchType.Joker)
					search.Enqueue(new Coordinate(c.x, c.y - 1));
			}
			if (c.y != GetGridHeight() - 1 && GetCell(c.x, c.y + 1) != null && TileUtility.TilesMatch(GetCell(c.x, c.y).MyTile, GetCell(c.x, c.y + 1).MyTile) && !adjacents.ContainsKey(GetCell(c.x, c.y + 1).MyTile)) {
				if ((GetCell(c.x, c.y + 1).MyTile as MatchableTile).MyMatchType == groupType || (GetCell(c.x, c.y + 1).MyTile as MatchableTile).MyMatchType == MatchType.Joker)
					search.Enqueue(new Coordinate(c.x, c.y + 1));
			}
		}
		return adjacents;
	}


	public void PopTiles(List<Dictionary<Tile, Coordinate>> tilesToPop, Callback TilesPopped) {
		StartCoroutine(TilePopRoutine(tilesToPop, TilesPopped));
	}
	public void PopTiles(Dictionary<Tile, Coordinate> tilesToPop, Callback TilesPopped) {
		StartCoroutine(TilePopRoutine(new List<Dictionary<Tile, Coordinate>>() { tilesToPop }, TilesPopped));
	}

	IEnumerator TilePopRoutine(List<Dictionary<Tile, Coordinate>> tilesToPop, Callback TilesPopped) {
		Tile chosen = null;
		Dictionary<Coordinate, MatchType> specialTilesToAdd = new Dictionary<Coordinate, MatchType>();
		foreach (Dictionary<Tile, Coordinate> p in tilesToPop) {
			chosen = null;
			if (CreateSpecialTiles) {
				if (p.Count >= ConstantHolder.specialTileThreshold) {
					foreach (Tile t in p.Keys) {
						if (t != null && t.CanPop)
							if (chosen == null || p[t].x < p[chosen].x || (p[t].x == p[chosen].x && p[t].y < p[chosen].y)) {
								chosen = t;
							}
					}
				}
				if (chosen != null) {
					specialTilesToAdd.Add(p[chosen], GetMostCommonMatchType(p));
				}
			}
			foreach (Tile tile in p.Keys) {
				tile.PopVisual(popVisualDuration);
				if (chosen != null && tile.CanPop) {
					tile.MoveToVisual(chosen.transform.position, popVisualDuration);
				}
			}
		}
		lowestPops.Clear();
		yield return new WaitForSeconds(popVisualDuration);
		foreach (Dictionary<Tile, Coordinate> p in tilesToPop) {
			foreach (Tile tile in p.Keys) {
				if (tile != GetCell(p[tile].x, p[tile].y).MyTile)
					continue;
				if (!lowestPops.ContainsKey(p[tile].x))
					lowestPops.Add(p[tile].x, p[tile].y);
				else if (lowestPops[p[tile].x] > p[tile].y)
					lowestPops[p[tile].x] = p[tile].y;
				PopTile(p[tile].x, p[tile].y);
			}
		}
		foreach (Coordinate c in specialTilesToAdd.Keys) {
			InstantiateTile(GetRandomSpecialTileToCreate(), c.x, c.y, false, new HashSet<MatchType>() { specialTilesToAdd[c] }, false).GrowVisual(popVisualDuration);
		}
		TilesPopped();
	}

	public int GetLargestMatchingGroupCount() {
		int largest = 1;
		List<Dictionary<Tile, Coordinate>> adjacents = GetAllTouchingMatches(2);
		foreach (Dictionary<Tile, Coordinate> group in adjacents) {
			if (group.Count > largest)
				largest = group.Count;
		}
		return largest + (CellCount - TileCount);
	}

	public Dictionary<Tile, Coordinate> GetAdjacentTiles(Tile t) {
		Cell c = null;
		for (int x = 0; x < GetGridWidth(); ++x) {
			for (int y = 0; y < GetGridHeight(); ++y) {
				c = GetCell(x, y);
				if (c != null && c.MyTile == t)
					return GetAdjacentTiles(new Coordinate(x, y));
			}
		}
		return null;
	}

	public Dictionary<Tile, Coordinate> GetAdjacentTiles(Coordinate c) {
		Dictionary<Tile, Coordinate> adjacents = new Dictionary<Tile, Coordinate>();
		Cell cell = null;
		Coordinate adjacentCoordinate = c;
		for (int i = 0; i < 4; ++i) {
			if (i == 0)
				adjacentCoordinate = new Coordinate(c.x + 1, c.y);
			else if (i == 1)
				adjacentCoordinate = new Coordinate(c.x - 1, c.y);
			else if (i == 2)
				adjacentCoordinate = new Coordinate(c.x, c.y + 1);
			else
				adjacentCoordinate = new Coordinate(c.x, c.y - 1);

			if (adjacentCoordinate.x >= 0 && adjacentCoordinate.x < grid.Length && adjacentCoordinate.y >= 0 && adjacentCoordinate.y < grid[adjacentCoordinate.x].Length)
				cell = GetCell(adjacentCoordinate.x, adjacentCoordinate.y);
			else
				cell = null;
			if (cell != null && cell.MyTile != null)
				adjacents.Add(cell.MyTile, new Coordinate(adjacentCoordinate.x, adjacentCoordinate.y));
		}
		return adjacents;
	}

	public List<Dictionary<Tile, Coordinate>> GetAllTouchingMatches(int minimumCombo) {
		List<Dictionary<Tile, Coordinate>> touchingMatches = new List<Dictionary<Tile, Coordinate>>();
		Dictionary<Tile, Coordinate> p;
		bool skip = false;
		for (int y = 0; y < GetGridHeight(); ++y) {
			for (int x = 0; x < GetGridWidth(); ++x) {
				if (GetCell(x, y) == null || GetCell(x, y).MyTile == null || (GetCell(x, y).MyTile is MatchableTile) && (GetCell(x, y).MyTile as MatchableTile).MyMatchType == MatchType.Joker)
					continue;
				skip = false;
				foreach (Dictionary<Tile, Coordinate> pops in touchingMatches) {
					if (pops.ContainsKey(GetCell(x, y).MyTile)) {
						skip = true;
						break;
					}
				}
				if (skip)
					continue;
				p = GetTouchingMatches(x, y);
				if (p.Count >= minimumCombo) {
					touchingMatches.Add(p);
				}
			}
		}
		return touchingMatches;
	}

	public List<Dictionary<Tile, Coordinate>> GetPostPopSpecialTilePops() {
		List<Dictionary<Tile, Coordinate>> tilesToPop = new List<Dictionary<Tile, Coordinate>>();
		Cell check = null;
		foreach (SpecialTileType stt in postPopSpecialTiles.Keys) {
			if (stt == SpecialTileType.Exploding) {
				foreach (Coordinate c in postPopSpecialTiles[stt]) {
					if (c.x > 0) {
						check = GetCell(c.x - 1, c.y);
						if (check != null && check.MyTile != null)
							tilesToPop.Add(new Dictionary<Tile, Coordinate>() { { check.MyTile, new Coordinate(c.x - 1, c.y) } });
						if (c.y > 0) {
							check = GetCell(c.x - 1, c.y - 1);
							if (check != null && check.MyTile != null)
								tilesToPop.Add(new Dictionary<Tile, Coordinate>() { { check.MyTile, new Coordinate(c.x - 1, c.y - 1) } });
						}
						if (c.y < grid[c.x].Length - 1) {
							check = GetCell(c.x - 1, c.y + 1);
							if (check != null && check.MyTile != null)
								tilesToPop.Add(new Dictionary<Tile, Coordinate>() { { check.MyTile, new Coordinate(c.x - 1, c.y + 1) } });
						}
					}
					if (c.x < grid.Length - 1) {
						check = GetCell(c.x + 1, c.y);
						if (check != null && check.MyTile != null)
							tilesToPop.Add(new Dictionary<Tile, Coordinate>() { { check.MyTile, new Coordinate(c.x + 1, c.y) } });

						if (c.y > 0) {
							check = GetCell(c.x + 1, c.y - 1);
							if (check != null && check.MyTile != null)
								tilesToPop.Add(new Dictionary<Tile, Coordinate>() { { check.MyTile, new Coordinate(c.x + 1, c.y - 1) } });
						}
						if (c.y < grid[c.x].Length - 1) {
							check = GetCell(c.x + 1, c.y + 1);
							if (check != null && check.MyTile != null)
								tilesToPop.Add(new Dictionary<Tile, Coordinate>() { { check.MyTile, new Coordinate(c.x + 1, c.y + 1) } });
						}
					}
					if (c.y > 0) {
						check = GetCell(c.x, c.y - 1);
						if (check != null && check.MyTile != null)
							tilesToPop.Add(new Dictionary<Tile, Coordinate>() { { check.MyTile, new Coordinate(c.x, c.y - 1) } });
					}
					if (c.y < grid[c.x].Length - 1) {
						check = GetCell(c.x, c.y + 1);
						if (check != null && check.MyTile != null)
							tilesToPop.Add(new Dictionary<Tile, Coordinate>() { { check.MyTile, new Coordinate(c.x, c.y + 1) } });
					}
				}
			}
			if (stt == SpecialTileType.Horizontal) {
				foreach (Coordinate c in postPopSpecialTiles[stt]) {
					for (int i = 0; i < GetGridWidth(); ++i) {
						if (i == c.x)
							continue;
						check = GetCell(i, c.y);
						if (check != null && check.MyTile != null)
							tilesToPop.Add(new Dictionary<Tile, Coordinate>() { { check.MyTile, new Coordinate(i, c.y) } });
					}
				}
			}
			if (stt == SpecialTileType.Vertical) {
				foreach (Coordinate c in postPopSpecialTiles[stt]) {
					for (int i = 0; i < GetGridHeight(); ++i) {
						if (i == c.y)
							continue;
						check = GetCell(c.x, i);
						if (check != null && check.MyTile != null)
							tilesToPop.Add(new Dictionary<Tile, Coordinate>() { { check.MyTile, new Coordinate(c.x, i) } });
					}
				}
			}
		}
		postPopSpecialTiles.Clear();
		return tilesToPop;
	}

	public List<Dictionary<Tile, Coordinate>> GetPostMoveSpecialTilePops() {
		List<Dictionary<Tile, Coordinate>> tilesToPop = new List<Dictionary<Tile, Coordinate>>();
		foreach (SpecialTileType stt in postMoveSpecialTiles.Keys) {
			if (stt == SpecialTileType.Exploding) {
				foreach (Tile t in postMoveSpecialTiles[stt]) {
					tilesToPop.Add(new Dictionary<Tile, Coordinate>() { { t, GetTileCoordinate(t) } });
					(t as ExplodingTile).Moved = true;
				}
			}
		}
		postMoveSpecialTiles.Clear();
		return tilesToPop;
	}

	void PopTile(Tile t) {
		Coordinate c = GetTileCoordinate(t);
		PopTile(c.x, c.y);
	}

	Coordinate GetTileCoordinate(Tile t) {
		for (int x = 0; x < GetGridWidth(); ++x) {
			for (int y = 0; y < GetGridHeight(); ++y) {
				if (GetCell(x, y) != null && GetCell(x, y).MyTile == t) {
					return new Coordinate(x, y);
				}
			}
		}
		Debug.LogError("Tile not found in coordinate search");
		return new Coordinate(-1, -1);
	}

	void PopTile(int x, int y) {
		Tile t = GetCell(x, y).MyTile;
		if (t.CanPop) {
			if (t is ISpecialTile)
				(t as ISpecialTile).TrySpecialPop(new Coordinate(x, y));
			t.Pop();
			PoolMaster.Instance.Destroy(t.gameObject);
			TileCount--;
			GetCell(x, y).MyTile = null;
		} else if (t is ISpecialTile)
			(t as ISpecialTile).TrySpecialPop(new Coordinate(x, y));
	}

	public void DropTiles(bool createNewTiles) {
		Cell c;
		bool foundTop, breakLoop;
		for (int y = 0; y < GetGridHeight(); ++y) {
			for (int x = 0; x < GetGridWidth(); ++x) {
				if (y >= GetGridHeight())
					break;
				int loopCount = 0;
				c = GetCell(x, y);
				if (c == null)
					continue;
				if (c.MyTile != null)
					DropTile(x, y);
				while (c.MyTile == null && loopCount < 1000) {
					loopCount++;
					foundTop = false;
					breakLoop = false;
					for (int i = 0; i < GetGridHeight() - y; ++i) {
						if (GetCell(x, y + i) == null) {
							breakLoop = true;
							foundTop = true;
							break;
						}
						if (GetCell(x, y + i).MyTile != null) {
							DropTile(x, y + i);
							foundTop = true;
							break;
						}
					}
					if (!foundTop) {
						if (createNewTiles) {
							InstantiateTile(matchableTilePrefab, x, GetGridHeight() - 1, true);
							AddTileMotion(x, GetGridHeight() - 1, GetCell(x, GetGridHeight() - 1));
							DropTile(x, GetGridHeight() - 1);
						} else
							breakLoop = true;
					}
					if (breakLoop)
						break;
				}
				if (loopCount >= 1000)
					Debug.LogError("Too loopy");
			}
		}
	}

	public void GrowTiles(float growPostWait, Dictionary<Tile, Coordinate> tilesToGrow, Callback GrowthDone) {
		GrowTiles(growPostWait, new List<Dictionary<Tile, Coordinate>>() { tilesToGrow }, GrowthDone);
	}

	public void GrowTiles(float growPostWait, List<Dictionary<Tile, Coordinate>> tilesToGrow, Callback GrowthDone) {
		Dictionary<Dictionary<Tile, Coordinate>, HashSet<MatchType>> excludedTypes = new Dictionary<Dictionary<Tile, Coordinate>, HashSet<MatchType>>();
		foreach (Dictionary<Tile, Coordinate> dic in tilesToGrow) {
			excludedTypes.Add(dic, new HashSet<MatchType>() { GetMostCommonMatchType(dic) });
		}
		GrowTiles(growPostWait, tilesToGrow, excludedTypes, true, GrowthDone);
	}

	public void GrowTiles(float growPostWait, List<Dictionary<Tile, Coordinate>> tilesToGrow, Dictionary<Dictionary<Tile, Coordinate>, HashSet<MatchType>> matchTypes, bool typesAreExcluded, Callback GrowthDone) {
		StartCoroutine(GrowthRoutine(growPostWait, tilesToGrow, matchTypes, typesAreExcluded, GrowthDone));
	}

	IEnumerator GrowthRoutine(float growPostWait, List<Dictionary<Tile, Coordinate>> tilesToGrow, Dictionary<Dictionary<Tile, Coordinate>, HashSet<MatchType>> matchTypes, bool typesAreExcluded, Callback GrowthDone) {
		while (!CanMove) {
			yield return null;
		}
		Cell c;
		foreach (Dictionary<Tile, Coordinate> dic in tilesToGrow) {
			foreach (Coordinate coor in dic.Values) {
				c = GetCell(coor.x, coor.y);
				if (c == null)
					continue;
				if (c.MyTile == null) {
					InstantiateTile(matchableTilePrefab, coor.x, coor.y, false, matchTypes[dic], typesAreExcluded).GrowVisual(popVisualDuration);
				}
			}
		}
		yield return new WaitForSeconds(popVisualDuration + growPostWait);
		GrowthDone();
	}

	public void MoveTiles(Callback MoveCompleted) {
		StartCoroutine(TileMoveRoutine(MoveCompleted));
	}

	IEnumerator TileMoveRoutine(Callback MoveCompleted) {
		int emptyQueues = 0;
		float motionRemaining;
		Vector3 startPos;
		float sign;
		speed = -jumpSpeed;
		Moving = true;
		Cell targetCell;
		Dictionary<Tile, float> tileSpeeds = new Dictionary<Tile, float>();
		Dictionary<Cell, Tile> currentTiles = new Dictionary<Cell, Tile>();
		SortedList<TileMotion, Tile> tileOrder = new SortedList<TileMotion, Tile>(new TilemotionComparer());
		foreach (Tile t in tileMotions.Keys) {
			tileOrder.Add(tileMotions[t], t);
		}
		while (emptyQueues < tileMotions.Count) {
			if (CanMove) {
				emptyQueues = 0;
				foreach (Tile t in tileOrder.Values) {
					if (tileMotions[t].Count == 0) {
						emptyQueues++;
						continue;
					}
					if (!currentTiles.ContainsKey(tileMotions[t].PrevCell)) {
						currentTiles.Add(tileMotions[t].PrevCell, t);
					}
					if (!tileSpeeds.ContainsKey(t))
						tileSpeeds.Add(t, -jumpSpeed);
					tileSpeeds[t] = Mathf.Min(maxFallSpeed, tileSpeeds[t] + Time.deltaTime * ConstantHolder.gravity);
					speed = tileSpeeds[t];
					sign = Mathf.Sign(speed);
					if (speed < 0 && (!lowestPops.ContainsKey(tileMotions[t].startX) || lowestPops[tileMotions[t].startX] > tileMotions[t].startY))
						continue;
					targetCell = tileMotions[t].PeekCell();
					if (!currentTiles.ContainsKey(targetCell))
						currentTiles.Add(targetCell, null);
					if (currentTiles[targetCell] == null) {
						currentTiles[targetCell] = t;
						if (tileMotions[t].PrevCell != targetCell) {
							currentTiles[tileMotions[t].PrevCell] = null;
						}
					}
					if (currentTiles[targetCell] != t) {
						if (tileMotions[t].PrevCell != targetCell)
							tileSpeeds[t] = Mathf.Min(tileSpeeds[t], 0);
						continue;
					}
					motionRemaining = Mathf.Abs(speed * Time.deltaTime);
					while (motionRemaining > 0 && tileMotions[t].Count > 0) {
						startPos = t.transform.position;
						t.transform.position = Vector3.MoveTowards(t.transform.position, tileMotions[t].PeekCell().transform.position, sign * motionRemaining);
						if (t.transform.position == targetCell.transform.position) {
							motionRemaining -= Vector3.Distance(startPos, tileMotions[t].Dequeue().transform.position);
						} else
							motionRemaining = 0;
					}

					if (tileMotions[t].Count == 0) {
						currentTiles[tileMotions[t].PrevCell] = null;
						emptyQueues++;
					}
				}
			}

			yield return null;
		}
		tileMotions.Clear();
		Moving = false;
		if (MoveCompleted != null)
			MoveCompleted();
	}

	void DropTile(int x, int y) {
		Cell c = GetCell(x, y);
		if (c == null) {
			Debug.LogError("No Cell for DropTile at x: " + x + " y: " + y);
			return;
		}
		Cell prev = c;
		Tile t = c.MyTile;
		if (t == null) {
			Debug.LogError("No Tile for DropTile at x: " + x + " y: " + y);
			return;
		}
		int fall = 1;
		int strafe = 0;
		bool loop = y - fall >= 0;
		while (loop) {
			c = GetCell(x + strafe, y - fall);
			loop = y - fall >= 0 && c != null && c.MyTile == null;
			if (loop) {
				AddTileMotion(x, y, c);
				c.MyTile = t;
				prev.MyTile = null;
				prev = c;
				y = y - fall;
				x = x + strafe;
				fall = 1;
				strafe = 0;
				loop = y - fall >= 0;
			} else if (y - fall + 1 > 0) {
				fall--;
				for (int i = 0; i < 2; ++i) {
					diagonalDirection *= -1;
					if (x + strafe + diagonalDirection < 0 || x + strafe + diagonalDirection >= GetGridWidth())
						continue;
					c = GetCell(x + strafe + diagonalDirection, y - fall);
					loop = y - fall > 0 && ((c != null && c.MyTile == null) || c == null);
					if (loop) {
						c = GetCell(x + strafe + diagonalDirection, y - fall - 1);
						loop = c != null && c.MyTile == null;
						if (loop) {
							fall++;
							strafe += diagonalDirection;
							break;
						}
					}

				}
			}
		}
	}

	Coordinate GetOriented(int x, int y) {
		if (Gravity == Direction.Down)
			return new Coordinate(x, y);
		else if (Gravity == Direction.Up)
			return new Coordinate(GetGridWidth() - 1 - x, GetGridHeight() - 1 - y);
		else if (Gravity == Direction.Right)
			return new Coordinate(GetGridHeight() - 1 - y, x);
		else
			return new Coordinate(y, GetGridWidth() - 1 - x);
	}

	Cell GetCell(int x, int y) {
		Coordinate c = GetOriented(x, y);
		return grid[c.x][c.y];
	}

	MatchType GetMostCommonMatchType(Dictionary<Tile, Coordinate> tileSet) {
		Dictionary<MatchType, int> typeCounts = new Dictionary<MatchType, int>();
		foreach (Tile t in tileSet.Keys) {
			if (t is MatchableTile) {
				if ((t as MatchableTile).MyMatchType == MatchType.Joker)
					continue;
				if (typeCounts.ContainsKey((t as MatchableTile).MyMatchType)) {
					typeCounts[(t as MatchableTile).MyMatchType]++;
				} else {
					typeCounts.Add((t as MatchableTile).MyMatchType, 1);
				}
			}
		}
		int max = 0;
		MatchType mostCommon = MatchType.None;
		foreach (MatchType m in typeCounts.Keys) {
			if (typeCounts[m] > max) {
				max = typeCounts[m];
				mostCommon = m;
			}
		}
		return mostCommon;
	}

	int GetGridWidth() {
		if (forceDownGravity || Gravity == Direction.Down || Gravity == Direction.Up)
			return gridSettings.gridWidth;
		else
			return gridSettings.gridHeight;
	}

	int GetGridHeight() {
		if (forceDownGravity || Gravity == Direction.Down || Gravity == Direction.Up)
			return gridSettings.gridHeight;
		else
			return gridSettings.gridWidth;
	}

	Vector3 GetGravityVector() {
		if (forceDownGravity)
			return Vector3.down;

		switch (Gravity) {
			case Direction.Down:
				return Vector3.down;
			case Direction.Up:
				return Vector3.up;
			case Direction.Left:
				return Vector3.left;
			default:
				return Vector3.right;
		}
	}

	public Cell GetRandomCell() {
		Cell toReturn = null;
		while (toReturn == null && CellCount > 0) {
			toReturn = grid.GetRandom().GetRandom();
		}
		return toReturn;
	}

	void AddTileMotion(int x, int y, Cell c) {
		Tile t = GetCell(x, y).MyTile;
		if (!tileMotions.ContainsKey(t)) {
			tileMotions.Add(t, new TileMotion());
			tileMotions[t].PrevCell = GetCell(x, y);
			tileMotions[t].PrevCoordinate = new Coordinate(x, y);
			tileMotions[t].startX = x;
			tileMotions[t].startY = y;
		}
		tileMotions[t].lastX = x;
		tileMotions[t].lastY = y;
		tileMotions[t].Enqueue(c, new Coordinate(x, y));
	}

	public void ClearGrid() {
		if (cellRoot != null) {
			for (int i = cellRoot.childCount - 1; i >= 0; --i) {
				cellRoot.GetChild(i).GetComponent<Cell>().MyTile = null;
				PoolMaster.Instance.Destroy(cellRoot.GetChild(i).gameObject);
			}
		}
		CellCount = 0;
		if (tileRoot != null) {
			for (int i = tileRoot.childCount - 1; i >= 0; --i)
				PoolMaster.Instance.Destroy(tileRoot.GetChild(i).gameObject);
		}
		TileCount = 0;
	}

	public void StopGrid() {
		StopAllCoroutines();
		tileMotions.Clear();
		ClearGrid();
		lowestPops.Clear();
	}

	public Sprite GetMatchSprite(MatchType type) {
		Sprite s = null;
		matchSprites.TryGetValue(type, out s);
		return s;
	}

	public GameObject GetMatchableTilePrefab() {
		return matchableTilePrefab.gameObject;
	}
}
