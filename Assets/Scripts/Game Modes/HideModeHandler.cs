using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideModeHandler : MonoBehaviour, IGameMode, ITileClickReceiver {
	
	protected int tileCount = -1;
	protected int hideCount = -1;
	protected int clicks = -1;
	protected Transform tileRoot;
	protected float randomDistance = 0;
	

	protected float popDuration = 0.2f;
	protected float tileSize = 0.9f;

	protected HashSet<MatchableTile> tiles = new HashSet<MatchableTile>();
	protected bool active;
	protected bool canClick;

	public void Activate() {

		clicks = 0;
		active = true;
		canClick = true;
		GameMaster.Instance.MaxProgress = tileCount;
		GameMaster.Instance.RemainingProgress = tileCount;
		MatchableTile tile;
		for (int i = 0; i < tileCount; ++i) {
			tile = PoolMaster.Instance.GetPooledObject(GridManager.GetManager().GetMatchableTilePrefab()).GetComponent<MatchableTile>();
			tiles.Add(tile);
			tile.Reset();
			//tile.transform.position = pathCreator.path.GetPointAtDistance(tiles[tile]);
			tile.Receiver = this;
			tile.SetRandomMatchType(6);
			tile.SetStencil(false);
			tile.transform.SetParent(transform);
			tile.transform.localScale = Vector3.one * tileSize;
		}
	}

	public void Back() {
		foreach (MatchableTile tile in tiles) {
			PoolMaster.Instance.Destroy(tile.gameObject);
		}
		tiles.Clear();
	}

	public bool CanClick() {
		return canClick;
	}

	public void ClickTile(Tile tile) {
		if (tile is MatchableTile && tiles.Contains((MatchableTile)tile)) {
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
		hideCount = -1;
		tileRoot = null;

		int holder = -1;
		foreach (LevelTypeSettings lts in levelTypes) {
			if (lts is IntSettings) {
				if (holder == -1)
					holder = ((IntSettings)lts).integer;
				else {
					tileCount = ((IntSettings)lts).integer;
					hideCount = Mathf.Max(tileCount, holder);
					tileCount = Mathf.Min(tileCount, holder);
				}
				continue;
			}
			if (lts is FloatSettings) {
				randomDistance = ((FloatSettings)lts).floatingPoint;
			}

			if (lts is TransformSettings) {
				tileRoot = ((TransformSettings)lts).transform;
			}
		}

		if (tileCount <= 0)
			Debug.LogError("Tilecount isn't positive!");
		if (hideCount <= 0)
			Debug.LogError("Hidecount isn't positive!");
		if (tileRoot = null)
			Debug.LogError("No tileroot!");
	}

	protected void ClickDustConversion() {
		GameMaster.Instance.SpaceDust += Mathf.RoundToInt(Mathf.Lerp(300, 0, (clicks - 10f) / 15f));
	}
	
	protected IEnumerator ClickRoutine(Tile tile) {
		canClick = false;
		tile.PopVisual(popDuration);
		tile.Pop();
		tiles.Remove((MatchableTile)tile);
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
