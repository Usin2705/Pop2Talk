using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "LootBox Settings")]
public class LootBoxSettings : ScriptableObject {
	public Sprite picture;
	public int price;
	public string[] cosmeticIds;
}
