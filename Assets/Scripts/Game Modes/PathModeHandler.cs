using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathModeHandler : MonoBehaviour, IGameMode, ITileClickReceiver, ICatcherTarget {

	protected float lowSpeed = -1;
	protected float highSpeed = -1;
	protected int tileCount = -1;
	protected PathCreation.PathCreator pathCreator;
	protected int clicks = -1;

	protected float popDuration = 0.3f;
	protected float tileSize = 0.9f;

	protected Dictionary<MatchableTile, float> pathTiles = new Dictionary<MatchableTile, float>();
	protected bool active;
	protected float routeTime;
	protected bool canClick;

	GameObject catcher;

	public void Activate() {
		clicks = 0;
		active = true;
		canClick = true;
		GameMaster.Instance.MaxProgress = tileCount;
		GameMaster.Instance.RemainingProgress = tileCount;
		float length = pathCreator.path.length;
		float tilePos;
		MatchableTile tile;
		bool tooClose;
		routeTime = 0;
		int attempts, maxAttempts = 500;
		for (int i = 0; i < tileCount; ++i) {
			tile = PoolMaster.Instance.GetPooledObject(GridManager.GetManager().GetMatchableTilePrefab()).GetComponent<MatchableTile>();
			attempts = 0;
			do {
				attempts++;
				tooClose = false;
				tilePos = Random.Range(0, length);
				foreach (MatchableTile t in pathTiles.Keys) {
					tooClose = Mathf.Abs(pathTiles[t] - tilePos) < tileSize || Mathf.Abs(pathTiles[t] - tilePos) > length - tileSize;
					if (tooClose)
						break;
				}
			} while (tooClose && attempts < maxAttempts);
			pathTiles.Add(tile, Random.Range(0, length));
			tile.Reset();
			tile.transform.position = pathCreator.path.GetPointAtDistance(pathTiles[tile]);
			tile.Receiver = this;
			tile.SetRandomMatchType(6);
			tile.SetStencil(false);
			tile.transform.SetParent(transform);
			tile.SetScale(Vector3.one * tileSize);
		}
	}

	public void Back() {
		foreach (MatchableTile tile in pathTiles.Keys) {
			PoolMaster.Instance.Destroy(tile.gameObject);
		}
		pathTiles.Clear();
		PoolMaster.Instance.Destroy(catcher);
		catcher = null;
		active = false;
	}

	public bool CanClick() {
		return canClick;
	}

	public void ClickTile(Tile tile) {
		if (tile is MatchableTile && pathTiles.ContainsKey((MatchableTile)tile)) {
			clicks++;
			StartCoroutine(ClickRoutine(tile));
		}
	}

	void Update() {
		if (!active)
			return;
		routeTime += Time.deltaTime * Mathf.Lerp(highSpeed, lowSpeed, pathTiles.Count/(float)tileCount);
		foreach (MatchableTile tile in pathTiles.Keys) {
			tile.transform.position = pathCreator.path.GetPointAtDistance(pathTiles[tile] + routeTime);
		}
	}

	public void Initialize(LevelSettings level) {
		GameMaster.Instance.SpaceDust = 0;
		if (catcher == null) {
			catcher = PoolMaster.Instance.GetPooledObject((GameObject)Resources.Load("Click Catcher"), transform);
			catcher.GetComponent<ClickCatcher>().CatcherTarget = this;
		}
		SetUpPathAndSpeeds(level.typeSettings);
	}

	protected void SetUpPathAndSpeeds(LevelTypeSettings[] levelTypes) {
		tileCount = -1;
		lowSpeed = -1;
		highSpeed = -1;
		pathCreator = null;
		float holder = -1;
		foreach (LevelTypeSettings lts in levelTypes) {
			if (lts is PathSettings) {
				pathCreator = ((PathSettings)lts).path;
				continue;
			}
			if (lts is IntSettings) {
				tileCount = ((IntSettings)lts).integer;
				continue;
			}
			if (lts is FloatSettings) {
				if (holder == -1) {
					holder = ((FloatSettings)lts).floatingPoint;
				} else {
					lowSpeed = (((FloatSettings)lts).floatingPoint);
					highSpeed = Mathf.Max(lowSpeed, holder);
					lowSpeed = Mathf.Min(lowSpeed, holder);
				}
				continue;
			}
		}

		if (tileCount <= 0) {
			Debug.LogError("Tilecount isn't positive!");
		}
		if (lowSpeed < 0) {
			Debug.LogError("Lowspeed is negative!");
		}
		if (highSpeed <= 0) {
			Debug.LogError("Highspeed is negative!");
		}
		if (pathCreator == null) {
			Debug.LogError("Path is null!");
		}
	}

	protected void ClickDustConversion() {
		GameMaster.Instance.SpaceDust += Mathf.RoundToInt(Mathf.Lerp(300, 0, (clicks - tileCount) / tileCount));
	}
	
	protected IEnumerator ClickRoutine(Tile tile) {
		canClick = false;
		tile.PopVisual(popDuration);
		tile.Pop();
		pathTiles.Remove((MatchableTile)tile);
		GameMaster.Instance.RemainingProgress--;
		GameMaster.Instance.Clicked();
		yield return new WaitForSeconds(popDuration);
		GameMaster.Instance.PlayPopSound(0);
		PoolMaster.Instance.Destroy(tile.gameObject);
		yield return new WaitForSeconds(GameMaster.Instance.GetPopSound().GetLength());
		GameMaster.Instance.ClickDone();
		if (pathTiles.Count == 0 && active) {
			active = false;
			ClickDustConversion();
			GameMaster.Instance.RoundDone();
		} else
			canClick = true;
	}

	public void CatchClick(Vector3 pos) {
		if (!active)
			return;
		AudioMaster.Instance.Play(this, SoundEffectManager.GetManager().GetBadClickSound());
		clicks++;
	}
}
