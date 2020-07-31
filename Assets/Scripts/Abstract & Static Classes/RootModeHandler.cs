﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RootModeHandler : MonoBehaviour, IGameMode {

	protected float minimumDistance = 0.8f;
	protected int clicks = -1;
	protected Transform rootTransform;
	protected bool shufflePositions;

	protected bool canClick;

	public bool CanClick() {
		return canClick;
	}

	public List<Vector3> GetPositions(int count) {
		List<Vector3> positions = new List<Vector3>();
		if (shufflePositions)
			count = Mathf.Max(count, rootTransform.childCount);
		Vector3 pos;
		bool tooClose;
		Transform hideNode;
		int attempts, maxAttempts = 100;
		bool skip = false;
		for (int i = 0; i < rootTransform.childCount; ++i) {
			if (positions.Count == count)
				break;
			hideNode = rootTransform.GetChild(i % rootTransform.childCount);
			if (hideNode.childCount != 0) {
				skip = true;
				continue;
			}
			positions.Add(hideNode.position);

			if (i == rootTransform.childCount - 1 && !skip)
				i = -1;
		}

		Dictionary<int, List<Vector3>> childCheck = new Dictionary<int, List<Vector3>>();

		for (int i = 0; positions.Count < count; ++i) {
			hideNode = rootTransform.GetChild(i % rootTransform.childCount);
			if (hideNode.childCount == 0) {
				continue;
			}
			if (!childCheck.ContainsKey(i)) {
				childCheck.Add(i, new List<Vector3>());
			}
				attempts = 0;
			do {
				tooClose = false;
				pos = new Vector3(Random.Range(hideNode.position.x, hideNode.GetChild(0).position.x),
					Random.Range(hideNode.position.y, hideNode.GetChild(0).position.y));
				foreach (Vector3 v in childCheck[i]) {
					if (Vector3.SqrMagnitude(v - pos) < minimumDistance * minimumDistance) {
						tooClose = true;
						break;
					}
				}
				attempts++;
			} while (tooClose && attempts < maxAttempts);
			childCheck[i].Add(pos);
			positions.Add(pos);
		}
		if (shufflePositions)
			positions.Shuffle();
		return positions;
	}

	public abstract void Activate();
	public abstract void Back();
	public abstract void Initialize(LevelSettings level);
}
