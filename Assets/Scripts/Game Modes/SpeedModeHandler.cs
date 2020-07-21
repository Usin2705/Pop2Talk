using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedModeHandler : RootModeHandler, ITileClickReceiver {

	protected int tileCount = -1;
	protected float tileTime = -1;
	protected float tileGap = -1;

	protected float tileSize = 1;
	float spawnTime = 0;


	protected Dictionary<MatchableTile, float> tiles = new Dictionary<MatchableTile, float>();
	protected HashSet<MatchableTile> shrinkingTiles = new HashSet<MatchableTile>();
	protected bool active;

	protected float timer;

	protected float popDuration = 0.2f;
	protected float growDuration = 0.3f;
	protected float startDelay = 1.5f;

	int currentlyPopping = 0;


	public override void Activate() {
		clicks = 0;
		active = true;
		canClick = true;
		currentlyPopping = 0;
		GameMaster.Instance.MaxProgress = tileCount;
		GameMaster.Instance.RemainingProgress = tileCount;
		tiles.Clear();
		shrinkingTiles.Clear();
		StartCoroutine(SpeedRoutine());
	}

	IEnumerator SpeedRoutine() {
		yield return new WaitForSeconds(startDelay);
		List<Vector3> positions = GetPositions(tileCount);
		float maxTime = (tileCount - 1) * tileGap + tileTime;
		spawnTime = 0;
		float resizeDuration = Mathf.Min(growDuration, tileTime / 2f);
		float shrinkAhead = Mathf.Max(tileTime - growDuration, tileTime / 2f);
		int spawnCount = 0;
		MatchableTile tile;
		List<MatchableTile> toRemove = new List<MatchableTile>();

		while (timer < maxTime) {
			timer += Time.deltaTime;
			while (timer > spawnTime && tileCount > spawnCount) {
				tile = PoolMaster.Instance.GetPooledObject(GridManager.GetManager().GetMatchableTilePrefab()).GetComponent<MatchableTile>();
				tiles.Add(tile, spawnTime);
				tile.SetStencil(false);
				tile.Reset();
				tile.Receiver = this;
				tile.SetRandomMatchType(6);
				tile.transform.SetParent(transform);
				tile.SetScale(Vector3.one * tileSize);
				tile.transform.position = positions[spawnCount];
				tile.GrowVisual(resizeDuration);
				spawnTime += tileGap;
				spawnCount++;
			}

			foreach (MatchableTile t in tiles.Keys) {
				if (!shrinkingTiles.Contains(t)) {
					if (tiles[t] + shrinkAhead < timer) {
						t.ShrinkVisual(timer + resizeDuration - (tiles[t] + shrinkAhead));
						shrinkingTiles.Add(t);
					}
				} else {
					if (tiles[t] + tileTime < timer) {
						PoolMaster.Instance.Destroy(t.gameObject);
						shrinkingTiles.Remove(t);
						toRemove.Add(t);
						GameMaster.Instance.RemainingProgress--;
					}
				}
			}

			foreach (MatchableTile t in toRemove) {
				tiles.Remove(t);
			}
			toRemove.Clear();
			yield return null;
		}
		ClickDustConversion();
		GameMaster.Instance.RoundDone();
	}

	public override void Back() {
		foreach (MatchableTile tile in tiles.Keys) {
			PoolMaster.Instance.Destroy(tile.gameObject);
		}
		tiles.Clear();
	}

	public void ClickTile(Tile tile) {
		if (tile is MatchableTile && tiles.ContainsKey((MatchableTile)tile)) {
			clicks++;
			tiles.Remove((MatchableTile)tile);
			tile.PopVisual(popDuration);
			StartCoroutine(TileDestruction(tile, popDuration));
			timer = spawnTime;
		}
	}

	IEnumerator TileDestruction(Tile t, float duration) {
		currentlyPopping++;
		GameMaster.Instance.Clicked();
		yield return new WaitForSeconds(duration);
		GameMaster.Instance.PlayPopSound(0);
		PoolMaster.Instance.Destroy(t.gameObject);
		GameMaster.Instance.RemainingProgress--;
		yield return new WaitForSeconds(GameMaster.Instance.GetPopSound().GetLength());
		currentlyPopping--;
		if (currentlyPopping == 0)
			GameMaster.Instance.ClickDone();
	}

	public override void Initialize(LevelSettings level) {
		GameMaster.Instance.SpaceDust = 0;
		SetupRootAndAmounts(level.typeSettings);
	}

	protected void SetupRootAndAmounts(LevelTypeSettings[] levelTypes) {
		tileCount = -1;
		tileTime = -1;
		tileGap = -1;
		rootTransform = null;
		shufflePositions = true;
		float holder = -1;

		foreach (LevelTypeSettings lts in levelTypes) {
			if (lts is IntSettings) {
				tileCount = ((IntSettings)lts).integer;
				continue;
			}

			if (lts is TransformSettings) {
				rootTransform = ((TransformSettings)lts).transform;
				continue;
			}

			if (lts is FloatSettings) {
				if (holder == -1) {
					holder = ((FloatSettings)lts).floatingPoint;
				} else {
					tileGap = ((FloatSettings)lts).floatingPoint;
					tileTime = Mathf.Max(tileGap, holder);
					tileGap = Mathf.Min(tileGap, holder);
				}
				continue;
			}

			if (lts is BoolSettings) {
				shufflePositions = ((BoolSettings)lts).boolean;
			}
		}

		if (tileCount <= 0)
			Debug.LogError("Tilecount isn't positive!");
		if (tileTime <= 0)
			Debug.LogError("Tiletime isn't positive!");
		if (tileGap <= 0)
			Debug.LogError("Tilegap isn't positive!");
		if (rootTransform == null)
			Debug.LogError("No tileroot!");
	}

	protected void ClickDustConversion() {
		GameMaster.Instance.SpaceDust += Mathf.RoundToInt(Mathf.Lerp(300, 0, clicks / (float)tileCount));
	}
}
