using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

[assembly: AssemblyVersion("1.0.*")]

public class BuildDateDisplayer : MonoBehaviour {

	private void Awake() {
		System.Version version = Assembly.GetExecutingAssembly().GetName().Version;
		System.DateTime startDate = new System.DateTime(2000, 1, 1, 0, 0, 0);
		System.TimeSpan span = new System.TimeSpan(version.Build, 0, 0, version.Revision * 2);
		System.DateTime buildDate = startDate.Add(span);
		GetComponentInChildren<Text>().text = buildDate.ToString("d MMMMM yyyy");
	}
}
