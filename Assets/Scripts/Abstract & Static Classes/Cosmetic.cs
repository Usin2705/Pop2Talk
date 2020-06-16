using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Cosmetic")]
public class Cosmetic : ScriptableObject {

	[SerializeField] CosmeticSlot slot;
	[SerializeField] Sprite icon;
	[SerializeField] Sprite sprite;
	[Space]
	[SerializeField][UniqueIdentifier] string uniqueId;
	[SerializeField] bool reset;

	public string Id { get; }

	public void OnValidate() {
		if (reset) {
			reset = false;
			uniqueId = "";
		}
	}
}
