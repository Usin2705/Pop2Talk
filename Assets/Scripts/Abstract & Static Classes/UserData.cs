using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserData {
    public string id;
    public string name;
    public bool consent;
    public string role;
	public GameState game_state;
	public string access_token;
}

public class GameState {
	public int largestModuleReceived; //The largest module that has been sent for the player.
	public Dictionary<string, int> wordHiscores; //words use their english name with a anguage prefix, aka en_uk_mom or fi_dad. Score is -1 for words that have not been said, 0-5 for others
	public int coins;
	public HashSet<string> unlockedCosmetics; //Cosmetics have a unique id
	public string[] equippedCosmetics; //Empty strings for slots with no equipment
	public int chosenCharacter; //represents the index of the characer in the array inside the game
}