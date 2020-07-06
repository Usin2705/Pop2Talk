using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideModeHandler : MonoBehaviour, IGameMode, ITileClickReceiver {
	
	protected int tileCount = -1;
	protected int totalCount = -1;
	protected int clicks = -1;
	protected Transform hideRoot;

	protected float popDuration = 0.2f;
	protected float tileSize = 0.75f;
	protected float mininmuDistance = 1.1f;

	protected HashSet<HideTile> tiles = new HashSet<HideTile>();
	protected bool active;
	protected bool canClick;

	GameObject hideTile;

	void Awake() {
		hideTile = Resources.Load("Hide Tile") as GameObject;
	}

	public void Activate() {
		clicks = 0;
		active = true;
		canClick = true;
		GameMaster.Instance.MaxProgress = tileCount;
		GameMaster.Instance.RemainingProgress = tileCount;
		HideTile tile;
		List<HideTile> toBeSet = new List<HideTile>();
		for (int i = 0; i < totalCount; ++i) {
			tile = PoolMaster.Instance.GetPooledObject(hideTile).GetComponent<HideTile>();
			toBeSet.Add(tile);
			tile.Reset();
			tile.Receiver = this;
			tile.SetRandomMatchType(6);
			tile.transform.SetParent(transform);
			tile.transform.localScale = Vector3.one * tileSize;
			tile.Hiding = i <= tileCount;
			
		}
		toBeSet.Shuffle();
		List<Vector3> positions = new List<Vector3>();
		Vector3 pos;
		bool tooClose;
		Transform hideNode;
		int attempts, maxAttempts = 100;
		for (int i = 0; i < toBeSet.Count; ++i) {
			attempts = 0;
			do {
				tooClose = false;
				hideNode = hideRoot.GetChild(i % hideRoot.childCount);
				if (hideNode.childCount == 0)
					pos = hideNode.position;
				else
					pos = Vector3.zero;
				foreach (Vector3 v in positions) {
					if (Vector3.SqrMagnitude(v - pos) < mininmuDistance * mininmuDistance) {
						tooClose = true;
						break;
					}
				}
				attempts++;
			} while (tooClose && attempts < maxAttempts);
			positions.Add(pos);
			toBeSet[i].transform.position = pos;
		}
		foreach (HideTile ht in toBeSet)
			tiles.Add(ht);
	}

	public void Back() {
		foreach (HideTile tile in tiles) {
			PoolMaster.Instance.Destroy(tile.gameObject);
		}
		tiles.Clear();
	}

	public bool CanClick() {
		return canClick;
	}

	public void ClickTile(Tile tile) {
		if (tile is MatchableTile && tiles.Contains((HideTile)tile)) {
			clicks++;
			StartCoroutine(ClickRoutine(tile));
		}
	}

	public void Initialize(LevelSettings level) {
		GameMaster.Instance.SpaceDust = 0;
		SetupRootAndAmounts(level.typeSettings);
	}

	protected void SetupRootAndAmounts(LevelTypeSettings[] levelTypes) {
		tileCount = -1;
		totalCount = -1;
		hideRoot = null;
		int holder = -1;
		foreach (LevelTypeSettings lts in levelTypes) {
			if (lts is IntSettings) {
				if (holder == -1)
					holder = ((IntSettings)lts).integer;
				else {
					tileCount = ((IntSettings)lts).integer;
					totalCount = Mathf.Max(tileCount, holder);
					tileCount = Mathf.Min(tileCount, holder);
				}
				continue;
			}

			if (lts is TransformSettings) {
				hideRoot = ((TransformSettings)lts).transform;
			}
		}

		if (tileCount <= 0)
			Debug.LogError("Tilecount isn't positive!");
		if (totalCount <= 0)
			Debug.LogError("Hidecount isn't positive!");
		if (hideRoot = null)
			Debug.LogError("No tileroot!");
	}

	protected void ClickDustConversion() {
		GameMaster.Instance.SpaceDust += Mathf.RoundToInt(Mathf.Lerp(300, 0, (clicks - tileCount) / Mathf.Max(1, totalCount-tileCount)));
	}
	
	protected IEnumerator ClickRoutine(Tile tile) {
		canClick = false;
		tile.PopVisual(popDuration);
		tile.Pop();
		tiles.Remove((HideTile)tile);
		GameMaster.Instance.RemainingProgress--;
		GameMaster.Instance.Clicked();
		yield return new WaitForSeconds(0.25f);
		GameMaster.Instance.PlayPopSound(0);
		PoolMaster.Instance.Destroy(tile.gameObject);
		yield return new WaitForSeconds(GameMaster.Instance.GetPopSound().GetLength());
		GameMaster.Instance.ClickDone();
		if (tiles.Count == 0) {
			active = false;
			GameMaster.Instance.RoundDone();
		} else
			canClick = true;
	}

	public void CatchClick() {
		AudioMaster.Instance.Play(this, SoundEffectManager.GetManager().GetBadClickSound());
		clicks++;
	}
}
