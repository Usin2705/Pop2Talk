using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Path Settings", menuName = "CustomObjects/Path Settings")]
public class PathSettings : LevelTypeSettings {
	public PathCreation.PathCreator path;
}
