﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionStatus : MonoBehaviour {

	[SerializeField] Text inGameName = null;
	[SerializeField] GameObject connectionIcon = null;
	[SerializeField] GameObject notConnectedIcon = null;

	public void SetIngameName(string name) {
		inGameName.text = name;
	}

	public void ShowName(bool show) {
		inGameName.gameObject.SetActive(show);
	}

	public void ToggleConnection(bool showConnection, bool connected) {
		connectionIcon.SetActive(showConnection);
		notConnectedIcon.SetActive(!connected && showConnection);
	}
}
