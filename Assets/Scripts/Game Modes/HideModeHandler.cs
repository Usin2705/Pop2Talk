using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideModeHandler : RootModeHandler, ITileClickReceiver {
	
	protected int tileCount = -1;
	protected int totalCount = -1;

	protected float popDuration = 0.3f;
	protected float endDuration = 0.6f;

	protected float tileSize = 0.75f;

	protected HashSet<HideTile> tiles = new HashSet<HideTile>();
	protected bool active;

	GameObject hideTile;

	float waitDuration = 1.5f;
	float growDuration = 2f;
	float revealDuration = 0.5f;

	public override void Activate() {
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
			tile.SetScale(Vector3.one * tileSize);
			tile.Reset();
			tile.Receiver = this;
			tile.SetRandomMatchType(6);
			tile.transform.SetParent(transform);
			tile.Hiding = i < tileCount;
			
		}
		toBeSet.Shuffle();
		List<Vector3> positions = GetPositions(toBeSet.Count);
		for (int i = 0; i < toBeSet.Count; ++i) {
			toBeSet[i].transform.position = positions[i];
			tiles.Add(toBeSet[i]);
		}

		StartCoroutine(OpenWait());
	}

	IEnumerator OpenWait() {
		canClick = false;
		foreach (HideTile ht in tiles) {
			ht.SetScale(Vector3.zero);
		}
		yield return new WaitForSeconds(revealDuration);
		foreach (HideTile ht in tiles) {
			ht.GrowReveal(revealDuration);
		}
		yield return new WaitForSeconds(waitDuration);
		foreach (HideTile ht in tiles) {
			ht.GrowVisual(growDuration);
		}
		yield return new WaitForSeconds(growDuration);
		canClick = true;
	}

	public override void Back() {
		foreach (HideTile tile in tiles) {
			PoolMaster.Instance.Destroy(tile.gameObject);
		}
		tiles.Clear();
	}

	public void ClickTile(Tile tile) {
		if (tile is HideTile && tiles.Contains((HideTile)tile)) {
			clicks++;
			StartCoroutine(ClickRoutine(tile));
		}
	}

	public override void Initialize(LevelSettings level) {
		GameMaster.Instance.SpaceDust = 0;
		if (hideTile == null)
			hideTile = Resources.Load("Hide Tile") as GameObject;
		SetupRootAndAmounts(level.typeSettings);
	}

	protected void SetupRootAndAmounts(LevelTypeSettings[] levelTypes) {
		tileCount = -1;
		totalCount = -1;
		rootTransform = null;
		shufflePositions = false;
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
				rootTransform = ((TransformSettings)lts).transform;
			}

			if (lts is BoolSettings) {
				shufflePositions = ((BoolSettings)lts).boolean;
			}
		}

		if (tileCount <= 0)
			Debug.LogError("Tilecount isn't positive!");
		if (totalCount <= 0)
			Debug.LogError("Hidecount isn't positive!");
		if (rootTransform == null)
			Debug.LogError("No tileroot!");
	}

	protected void ClickDustConversion() {
		GameMaster.Instance.SpaceDust += Mathf.RoundToInt(Mathf.Lerp(300, 0, (clicks - tileCount) / (float)Mathf.Max(1, totalCount-tileCount)));
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
				/*if (t.CanPop) {
					t.PopVisual(endDuration);
				}
				if (((HideTile)t).Hiding) {*/
					((HideTile)t).FadeHide(endDuration);
				//}
			}
			yield return new WaitForSeconds(endDuration);
			foreach (Tile t in tiles) {
				PoolMaster.Instance.Destroy(t.gameObject);
			}
			tiles.Clear();
			ClickDustConversion();
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
