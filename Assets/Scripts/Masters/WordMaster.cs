using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordMaster : MonoBehaviour {

	static WordMaster instance;

	//string path = "Assets/Word Data/Data Objects/Resources/";

	public static WordMaster Instance {
		get {
			if (instance == null) {
				instance = new GameObject("Word Master").AddComponent<WordMaster>();
			}

			return instance;
		}
	}

	public int TotalWords {
		get {
			return allWords.Count;
		}
	}

	public int CardsRemaining {
		get {
			return (cardTypeQueue == null) ? 0 : cardTypeQueue.Count;
		}
	}

	public int MaxCards {
		get {
			return (sampleWords != null) ? sampleWords.Length : 0;
		}
	}

	public int LargestModuleIndex { get; protected set; } = 1;

	public int TotalStars { get; set; }
	public bool OnlyMemory { get; protected set; }

	Queue<WordCardType> cardTypeQueue;
	Queue<WordData> wordQueue;

	WordCardType[] sampleTypes;
	string[] sampleWords;

	Dictionary<string, int> bestStars = new Dictionary<string, int>();
	HashSet<string> allWords = new HashSet<string>();

	BaseWordCardHandler currentWordCardHandler;
	Dictionary<WordCardType, BaseWordCardHandler> wordCardHandlers = new Dictionary<WordCardType, BaseWordCardHandler>();
	Dictionary<string, WordData> stringWordDataDictionary = new Dictionary<string, WordData>();

	float starMinimum = 1.1f;


	public void Dequeue() {
		cardTypeQueue.Dequeue();
		wordQueue.Dequeue();
	}

	public WordCardType PeekNextType() {
		return cardTypeQueue.Peek();
	}

	public WordData PeekNextWord() {
		return wordQueue.Peek();
	}

	public void ClearWords() {
		bestStars.Clear();
		allWords.Clear();
	}

	public void AddWord(string word) {
		if (allWords.Contains(word))
			Debug.Log("Allwords already contains " + word + "!");
		allWords.Add(word);
		/*if (UnityEditor.AssetDatabase.AssetPathToGUID(path + word + ".asset") == "")
			Debug.Log("No asset for " + word + "!");*/
	}

	public void SetStarAmount(string word, int star) {
		if (!allWords.Contains(word))
			return;
		if (star >= 0) {
			if (!bestStars.ContainsKey(word)) 
				bestStars.Add(word, star);
			else {
				bestStars[word] = star;
			}
		} else if (bestStars.ContainsKey(word)) {
			bestStars.Remove(word);
		}

	}

	public void SetStarAmounts(string[] words, int[] stars) {
		for (int i = 0; i < words.Length; ++i) {
			SetStarAmount(words[i], stars[i]);
		}
	}

	public bool RecordStarAmount(string word, int stars, int type) {
		NetworkManager.GetManager().SendBestCardStar(word, stars, type);
		if (!allWords.Contains(word)) {
			return false;
		}
		if (!bestStars.ContainsKey(word))
			bestStars.Add(word, -1);
		bool improvement = stars > bestStars[word];
		if (improvement) {
			bestStars[word] = stars;
		}
		return improvement;
	}

	public Dictionary<string, int> GetBestResults() {
		return bestStars;
	}

	public void MakeQueue() {
		cardTypeQueue = new Queue<WordCardType>();
		wordQueue = new Queue<WordData>();
		foreach (WordCardType wct in sampleTypes) {
			cardTypeQueue.Enqueue(wct);
		}
		foreach (string s in sampleWords) {
			wordQueue.Enqueue(StringToWordData(s));
		}
	}

	public void SetSamples(WordCardType[] types, string[] words) {
		sampleTypes = types;
		sampleWords = words;
		OnlyMemory = true;
		foreach(WordCardType wct in types) {
			if (wct != WordCardType.Memory) {
				OnlyMemory = false;
				break;
			}
		}
	}

	public WordData StringToWordData(string word) {
		if (stringWordDataDictionary.ContainsKey(word))
			return stringWordDataDictionary[word];
		WordData data = (Resources.Load(word) as WordData);
		if (data != null) {
			stringWordDataDictionary.Add(word, data);
		} else {
			Debug.Log(word);
		}
		return data;
	}

	public void ShowWordCard(WordCardType type, string levelName, WordData word, int order, IntCallback Done) {
		if (!wordCardHandlers.ContainsKey(type)) {
			switch (type) {
				case WordCardType.Repeat:
					wordCardHandlers.Add(type, new RepeatCardHandler());
					break;
				case WordCardType.Memory:
					wordCardHandlers.Add(type, new MemoryCardHandler());
					break;
			}
		}
		wordCardHandlers[type].ShowCard(word, levelName, order, Done);
	}

	public float GetStarRatio(float starAverage) {
		return Mathf.Clamp01((starAverage - starMinimum) / (5 - starMinimum));
	}

	public int GetHighScore(string word) {
		if (!bestStars.ContainsKey(word))
			return -1;
		return bestStars[word];
	}

	public void SetLargestModuleIndex(int module) {
		if (module > LargestModuleIndex)
			LargestModuleIndex = module;
	}
}
