﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Cosmetic", menuName = "CustomObjects/Cosmetic")]
public class Cosmetic : ScriptableObject {

	public CosmeticSlot slot;
	public Sprite icon;
	public Sprite sprite;
	public Sprite[] extraSprites;
	[Space]
	[SerializeField] string idCopy;
	[SerializeField][UniqueIdentifier] string uniqueId;
	[SerializeField] bool reset;

	public string Id { get { return uniqueId; } }

	public void OnValidate() {
		if (idCopy == "") {
			idCopy = uniqueId;
		}
		if (reset) {
			reset = false;
			uniqueId = "";
			idCopy = "";
		}

	}
}
