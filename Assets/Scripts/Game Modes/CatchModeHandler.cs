using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchModeHandler : MonoBehaviour, IGameMode, ICatcherTarget {

	protected int tileCount = -1;
	protected float tileGap = -1;
	protected float tileSpeed = -1;
	protected Transform rootTransform;

	protected float tileSize = 1;

	protected Dictionary<MatchableTile, int> tiles = new Dictionary<MatchableTile, int>();
	protected Transform[] portals;
	protected bool active;

	protected int collectedTiles;
	protected int poppedTiles;
	protected int tilesPlaying;

	protected int targetIndex;

	protected float popDuration = 0.2f;
	protected float growDuration = 0.25f;
	protected float startDelay = 1.5f;
	protected float tileCatcherSpeed = 11f;

	GameObject tileCatcher;
	GameObject clickCatcher;

	public void Activate() {
		collectedTiles = 0;
		poppedTiles = 0;
		tilesPlaying = 0;
		active = true;
		GameMaster.Instance.MaxProgress = tileCount;
		GameMaster.Instance.RemainingProgress = tileCount;
		StartCoroutine(TileDropRoutine());
	}

	public void Back() {
		foreach (MatchableTile mt in tiles.Keys) {
			PoolMaster.Instance.Destroy(mt.gameObject);
		}
		tiles.Clear();
		if (portals != null) {
			for (int i = 0; i < portals.Length; ++i) {
				Destroy(portals[i].gameObject);
			}
		}
		portals = null;
		PoolMaster.Instance.Destroy(clickCatcher);
		PoolMaster.Instance.Destroy(tileCatcher);
		clickCatcher = null;
		tileCatcher = null;
		active = false;
	}

	public void Initialize(LevelSettings level) {
		GameMaster.Instance.SpaceDust = 0;
		gameObject.SetActive(true);
		SetupRootAndAmounts(level.typeSettings);
		CreatePortals(rootTransform.GetChild(0));

		if (clickCatcher == null) {
			clickCatcher = PoolMaster.Instance.GetPooledObject((GameObject)Resources.Load("Click Catcher"), transform);
			clickCatcher.GetComponent<ClickCatcher>().CatcherTarget = this;
		} else
			clickCatcher.SetActive(true);

		if (tileCatcher == null) {
			tileCatcher = PoolMaster.Instance.GetPooledObject(Resources.Load("Tile Catcher") as GameObject, transform);
			tileCatcher.GetComponent<TileCatcher>().SetCatchModeHandler(this);
		} else
			tileCatcher.SetActive(true);

		tileCatcher.transform.position = rootTransform.GetChild(1).transform.position;
		targetIndex = portals.Length / 2;//intentional int division
		tileCatcher.transform.position = new Vector3(portals[targetIndex].position.x, rootTransform.GetChild(1).transform.position.y, rootTransform.GetChild(1).transform.position.z);
	}

	IEnumerator TileDropRoutine() {
		yield return new WaitForSeconds(1f);
		Vector3 targetPosition;
		float timer = 0;
		int index;
		Dictionary<MatchableTile, float> tileSpawnTimes = new Dictionary<MatchableTile, float>();
		while (poppedTiles < tileCount || tiles.Count > 0) {
			timer += Time.deltaTime;

			foreach (MatchableTile mt in tiles.Keys) {
				if (!mt.CanPop || tileSpawnTimes[mt] + growDuration > Time.time)
					continue;
				mt.transform.position = Vector3.MoveTowards(mt.transform.position, rootTransform.GetChild(2).GetChild(tiles[mt]).position, tileSpeed * Time.deltaTime);
				if (Vector3.SqrMagnitude(mt.transform.position - rootTransform.GetChild(2).GetChild(tiles[mt]).position) <= Mathf.Epsilon) {
					StartCoroutine(PopRoutine(mt));
				}
			}

			while (timer > tileGap && poppedTiles < tileCount) {
				index = Random.Range(0, portals.Length);
				timer -= tileGap;
				MatchableTile mt = PoolMaster.Instance.GetPooledObject(GridManager.GetManager().GetMatchableTilePrefab(), transform).GetComponent<MatchableTile>();
				mt.Reset();
				mt.transform.position = portals[index].position;
				mt.SetRandomMatchType(6);
				mt.SetStencil(false);
				mt.GrowVisual(growDuration);
				if (!tileSpawnTimes.ContainsKey(mt))
					tileSpawnTimes.Add(mt, 0);
				tileSpawnTimes[mt] = Time.time;
				tiles.Add(mt, index);
			}

			targetPosition = new Vector3(portals[targetIndex].position.x, tileCatcher.transform.position.y, tileCatcher.transform.position.z);
			tileCatcher.transform.position = Vector3.MoveTowards(tileCatcher.transform.position, targetPosition, tileCatcherSpeed * Time.deltaTime);
			yield return null;
		}
		yield return new WaitForSeconds(1f);
		active = false;
		CatchDustConversion();
		GameMaster.Instance.RoundDone();
	}

	IEnumerator PopRoutine(MatchableTile mt) {
		mt.Pop();
		mt.ShrinkVisual(popDuration);
		GameMaster.Instance.RemainingProgress--;
		yield return null; //Wait for next frame to not break enumeration
		tiles.Remove(mt);
		yield return new WaitForSeconds(popDuration);
		PoolMaster.Instance.Destroy(mt.gameObject);
		poppedTiles++;
	}

	public void CreatePortals(Transform root) {
		portals = new Transform[root.childCount];
		GameObject portalPrefab = Resources.Load("Catch Portal") as GameObject;
		GameObject portal;
		for (int i = 0; i < root.childCount; ++i) {
			portal = Instantiate(portalPrefab, transform);
			portal.transform.position = root.GetChild(i).position;
			portals[i] = portal.transform;
		}
	}

	protected void SetupRootAndAmounts(LevelTypeSettings[] levelTypes) {
		tileCount = -1;
		tileGap = -1;
		tileSpeed = -1;
		rootTransform = null;
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
					tileSpeed = Mathf.Max(tileGap, holder);
					tileGap = Mathf.Min(tileGap, holder);
				}
				continue;
			}
		}

		if (tileCount <= 0)
			Debug.LogError("Tilecount isn't positive!");
		if (tileSpeed <= 0)
			Debug.LogError("Tilespeed isn't positive!");
		if (tileGap <= 0)
			Debug.LogError("Tilegap isn't positive!");
		if (rootTransform == null)
			Debug.LogError("No tileroot!");
	}

	protected void CatchDustConversion() {
		GameMaster.Instance.SpaceDust += Mathf.RoundToInt(Mathf.Lerp(300, 0, (tileCount - collectedTiles) / (float)tileCount));
	}

	public void CatchClick(Vector3 pos) {
		if (!active)
			return;
		float shortestDistance = float.MaxValue;
		float distance;
		int chosenIndex = -1;

		for (int i = 0; i < portals.Length; ++i) {
			distance = Mathf.Abs(pos.x - portals[i].position.x);
			if (distance < shortestDistance) {
				shortestDistance = distance;
				chosenIndex = i;
			}
		}

		if (targetIndex == chosenIndex) {
			shortestDistance = 0.8f;

			for (int i = 0; i < portals.Length; ++i) {
				if (i == targetIndex)
					continue;
				distance = Mathf.Abs(pos.x - portals[i].position.x);
				if (distance < shortestDistance) {
					shortestDistance = distance;
					chosenIndex = i;
				}
			}
		}

		if (chosenIndex != -1)
			targetIndex = chosenIndex;
	}

	public void CatchTile(MatchableTile mt) {
		if (mt != null && mt.CanPop && tiles.ContainsKey(mt)) {
			StartCoroutine(PopRoutine(mt));
			StartCoroutine(CatchRoutine(mt));
		}
	}

	IEnumerator CatchRoutine(MatchableTile mt) {
		if (tilesPlaying == 0)
			GameMaster.Instance.Clicked();
		tilesPlaying++;
		collectedTiles++;
		float a = 0;
		GameMaster.Instance.PlayPopSound(0);
		while (a < popDuration) {
			mt.transform.position = Vector3.MoveTowards(mt.transform.position, tileCatcher.transform.position, tileCatcherSpeed * Time.deltaTime);
			a += Time.deltaTime;
			yield return null;
		}
		yield return new WaitForSeconds(Mathf.Max(0,GameMaster.Instance.GetPopSound().GetLength()-popDuration));
		tilesPlaying--;
		if (tilesPlaying == 0 && active)
			GameMaster.Instance.ClickDone();
	}

}
