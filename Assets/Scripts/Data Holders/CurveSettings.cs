using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Curve Settings", menuName = "CustomObjects/Curve Settings")]
public class CurveSettings : LevelTypeSettings {
	public AnimationCurve curve;
}
