using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGameModeHandler : MonoBehaviour, IGameMode, ITileClickReceiver {

	protected float lowSpeed = -1;
	protected float highSpeed = -1;
	protected int tileCount = -1;
	protected PathCreation.PathCreator path;
	protected int clicks = -1;
	//protected Dictionary<PathTile, float> pathTiles;

	public void Activate() {
		clicks = -1;
	}

	public void Back() {
	}

	public bool CanClick() {
		return true;
	}

	public void ClickTile(Tile t) {
		throw new System.NotImplementedException();
	}

	public void Initialize(LevelSettings level) {
		
		GameMaster.Instance.SpaceDust = 0;
		//SetUpPathAndSpeeds(level);
	}

	protected void SetUpPathAndSpeeds(LevelTypeSettings[] levelTypes) {
		tileCount = -1;
		lowSpeed = -1;
		highSpeed = -1;
		path = null;
		foreach(LevelTypeSettings lts in levelTypes){

		}
	}
}
