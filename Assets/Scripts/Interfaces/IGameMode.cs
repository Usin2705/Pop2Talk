using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameMode {

	void Initialize(LevelSettings level);
	void Activate();
	void Back();
	bool GetMedal(int value, int target);
}
