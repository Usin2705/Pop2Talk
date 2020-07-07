using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideModeHandler : MonoBehaviour, IGameMode, ITileClickReceiver {
	
	protected int tileCount = -1;
	protected int totalCount = -1;
	protected int clicks = -1;
	protected Transform hideRoot;

	protected float popDuration = 0.3f;
	protected float endDuration = 0.6f;
	protected float tileSize = 0.85f;
	protected float mininmuDistance = 0.7f;

	protected HashSet<HideTile> tiles = new HashSet<HideTile>();
	protected bool active;
	protected bool canClick;

	GameObject hideTile;

	float waitDuration = 1.5f;
	float growDuration = 1.5f;

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
			tile.Hiding = i < tileCount;
			
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
				else {
					pos = new Vector3(Random.Range(hideNode.position.x, hideNode.GetChild(0).position.x), 
						Random.Range(hideNode.position.y, hideNode.GetChild(0).position.y));
				}
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

		StartCoroutine(OpenWait(waitDuration));
	}

	IEnumerator OpenWait(float duration) {
		canClick = false;
		yield return new WaitForSeconds(waitDuration);
		foreach (Tile t in tiles) {
			t.GrowVisual(growDuration);
		}
		yield return new WaitForSeconds(growDuration);
		canClick = true;
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
		if (tile is HideTile && tiles.Contains((HideTile)tile)) {
			clicks++;
			StartCoroutine(ClickRoutine(tile));
		}
	}

	public void Initialize(LevelSettings level) {
		GameMaster.Instance.SpaceDust = 0;
		if (hideTile == null)
			hideTile = Resources.Load("Hide Tile") as GameObject;
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
		if (hideRoot == null)
			Debug.LogError("No tileroot!");
	}

	protected void ClickDustConversion() {
		GameMaster.Instance.SpaceDust += Mathf.RoundToInt(Mathf.Lerp(300, 0, (clicks - tileCount) / Mathf.Max(1, totalCount-tileCount)));
	}
	
	protected IEnumerator ClickRoutine(Tile tile) {
		canClick = false;
		tile.PopVisual(popDuration);
		tile.Pop();
		if (((HideTile)tile).Hiding)
			GameMaster.Instance.RemainingProgress--;
		GameMaster.Instance.Clicked();
		yield return new WaitForSeconds(popDuration);
		((HideTile)tile).HideHider();
		if (popDuration < 0.25f)
			yield return new WaitForSeconds(0.25f-popDuration);
		GameMaster.Instance.PlayPopSound(0);
		yield return new WaitForSeconds(GameMaster.Instance.GetPopSound().GetLength());
		GameMaster.Instance.ClickDone();
		if (GameMaster.Instance.RemainingProgress == 0) {
			foreach(Tile t in tiles) {
				if (t.CanPop) {
					t.PopVisual(endDuration);
				}
				if (((HideTile)t).Hiding) {
					((HideTile)t).FadeHide(endDuration);
				}
			}
			yield return new WaitForSeconds(endDuration);
			foreach (Tile t in tiles) {
				PoolMaster.Instance.Destroy(t.gameObject);
			}
			tiles.Clear();
			yield return new WaitForSeconds(0.25f);
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
