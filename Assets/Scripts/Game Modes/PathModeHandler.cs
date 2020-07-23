using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathModeHandler : MonoBehaviour, IGameMode, ITileClickReceiver, ICatcherTarget {

	protected float lowSpeed = -1;
	protected float highSpeed = -1;
	protected int tileCount = -1;
	protected PathCreation.PathCreator pathCreator;
	protected int clicks = -1;
	protected AnimationCurve speedCurve;

	protected float popDuration = 0.3f;
	protected float tileSize = 0.9f;

	protected Dictionary<MatchableTile, float> tilePositions = new Dictionary<MatchableTile, float>();
	protected HashSet<MatchableTile> pathTiles = new HashSet<MatchableTile>();
	protected bool active;
	protected bool canClick;
	protected int activeClicks;

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
		int attempts, maxAttempts = 1000;
		float minDistance = tileSize * 1.5f;
		for (int i = 0; i < tileCount; ++i) {
			tile = PoolMaster.Instance.GetPooledObject(GridManager.GetManager().GetMatchableTilePrefab()).GetComponent<MatchableTile>();
			attempts = 0;
			do {
				attempts++;
				tooClose = false;
				tilePos = Random.Range(0, length);
				foreach (MatchableTile t in tilePositions.Keys) {
					tooClose = Mathf.Abs(tilePositions[t] - tilePos) < minDistance || Mathf.Abs(tilePositions[t] - tilePos) > length - minDistance;
					if (tooClose)
						break;
				}
			} while (tooClose && attempts < maxAttempts);
			if (attempts >= maxAttempts)
				Debug.Log("too loopy");
			pathTiles.Add(tile);
			tilePositions.Add(tile, tilePos);
			tile.Reset();
			tile.transform.position = pathCreator.path.GetPointAtDistance(tilePositions[tile]);
			tile.Receiver = this;
			tile.SetRandomMatchType(6);
			tile.SetStencil(false);
			tile.transform.SetParent(transform);
			tile.SetScale(Vector3.one * tileSize);
			tile.GrowVisual(0.33f);
		}
	}

	public void Back() {
		foreach (MatchableTile tile in tilePositions.Keys) {
			PoolMaster.Instance.Destroy(tile.gameObject);
		}
		tilePositions.Clear();
		pathTiles.Clear();
		PoolMaster.Instance.Destroy(catcher);
		catcher = null;
		active = false;
	}

	public bool CanClick() {
		return canClick;
	}

	public void ClickTile(Tile tile) {
		if (tile is MatchableTile && tilePositions.ContainsKey((MatchableTile)tile)) {
			clicks++;
			StartCoroutine(ClickRoutine(tile));
		}
	}

	void Update() {
		if (!active)
			return;
		float speed = Time.deltaTime * Mathf.Lerp(highSpeed, lowSpeed, tilePositions.Count / (float)tileCount);
		foreach (MatchableTile tile in pathTiles) {
			tilePositions[tile] += speed * ((speedCurve == null) ? 1 : speedCurve.Evaluate(tilePositions[tile] / pathCreator.path.length));
			tile.transform.position = pathCreator.path.GetPointAtDistance(tilePositions[tile]);
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
		speedCurve = null;

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
			if (lts is CurveSettings)
				speedCurve = ((CurveSettings)lts).curve;
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
		GameMaster.Instance.SpaceDust += Mathf.RoundToInt(Mathf.Lerp(300, 0, (clicks - tileCount) / (float)tileCount));
	}

	protected IEnumerator ClickRoutine(Tile tile) {
		if (activeClicks == 0)
			GameMaster.Instance.Clicked();
		activeClicks++;
		tile.PopVisual(popDuration);
		tile.Pop();
		tilePositions.Remove((MatchableTile)tile);
		pathTiles.Remove((MatchableTile)tile);
		GameMaster.Instance.RemainingProgress--;
		yield return new WaitForSeconds(popDuration);
		GameMaster.Instance.PlayPopSound(activeClicks-1);
		PoolMaster.Instance.Destroy(tile.gameObject);
		yield return new WaitForSeconds(GameMaster.Instance.GetPopSound().GetLength());
		activeClicks--;
		if (activeClicks == 0) {
			GameMaster.Instance.ClickDone();
			if (tilePositions.Count == 0 && active) {
				active = false;
				canClick = false;
				ClickDustConversion();
				GameMaster.Instance.RoundDone();
			}
		}
	}

	public void CatchClick(Vector3 pos) {
		if (!active)
			return;
		AudioMaster.Instance.Play(this, SoundEffectManager.GetManager().GetBadClickSound());
		clicks++;
	}
}
