﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RootModeHandler : MonoBehaviour, IGameMode {
	
	protected float minimumDistance = 0.8f;
	protected int clicks = -1;
	protected Transform rootTransform;

	protected bool canClick;

	public bool CanClick() {
		return canClick;
	}

	public List<Vector3> GetPositions(int count) {
		List<Vector3> positions = new List<Vector3>();
		Vector3 pos;
		bool tooClose;
		Transform hideNode;
		int attempts, maxAttempts = 100;
		for (int i = 0; i < count; ++i) {
			attempts = 0;
			do {
				tooClose = false;
				hideNode = rootTransform.GetChild(i % rootTransform.childCount);
				if (hideNode.childCount == 0)
					pos = hideNode.position;
				else {
					pos = new Vector3(Random.Range(hideNode.position.x, hideNode.GetChild(0).position.x),
						Random.Range(hideNode.position.y, hideNode.GetChild(0).position.y));
				}
				foreach (Vector3 v in positions) {
					if (Vector3.SqrMagnitude(v - pos) < minimumDistance * minimumDistance) {
						tooClose = true;
						break;
					}
				}
				attempts++;
			} while (tooClose && attempts < maxAttempts);
			positions.Add(pos);
		}
		return positions;
	}

	public abstract void Activate();
	public abstract void Back();
	public abstract void Initialize(LevelSettings level);
}
