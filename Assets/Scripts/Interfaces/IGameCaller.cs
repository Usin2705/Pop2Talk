using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameCaller {
	void RoundDone();
	void Clicked();
	void ClickDone();
	void LaunchSetup();
	void SetProgress(float progress);
	void SetTrackedValue(int value);
	void SetSpecificUI(List<MatchType> matchTypes);
	int GetMaxRounds();
}
