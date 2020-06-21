using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordMaster : MonoBehaviour {

	static WordMaster instance;

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
			return bestStars.Count + unsaidWords.Count;
		}
	}

	public int CardsRemaining {
		get {
			return (cardTypeQueue == null) ? 0 : cardTypeQueue.Count;
		}
	}

	public int MaxCards {
		get {
			return sampleWords.Length;
		}
	}

	public int TotalStars { get; set; }

	Queue<WordCardType> cardTypeQueue;
	Queue<WordData> wordQueue;

	WordCardType[] sampleTypes;
	string[] sampleWords;
	float par;

	Dictionary<string, int> bestStars = new Dictionary<string, int>();
	HashSet<string> unsaidWords = new HashSet<string>();

	BaseWordCardHandler currentWordCardHandler;
	Dictionary<WordCardType, BaseWordCardHandler> wordCardHandlers = new Dictionary<WordCardType, BaseWordCardHandler>();
	Dictionary<string, WordData> stringWordDataDictionary = new Dictionary<string, WordData>();


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

	public void SetStarAmounts(string[] words, int[] stars) {
		for (int i = 0; i < words.Length; ++i) {
			if (stars[i] >= 0)
				bestStars.Add(words[i], stars[i]);
			else
				unsaidWords.Add(words[i]);
		}
	}

	public bool RecordStarAmount(string word, int stars) {
		if (unsaidWords.Contains(word)) {
			unsaidWords.Remove(word);
			bestStars.Add(word, -1);
		}
		bool improvement = stars > bestStars[word];
		bestStars[word] = stars;
		return improvement;
	}

	public Dictionary<string, int> GetBestResults() {
		return bestStars;
	}

	public int GetUnsaidWordCount() {
		return unsaidWords.Count;
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

	public void SetSamples(WordCardType[] types, string[] words, float par) {
		sampleTypes = types;
		sampleWords = words;
		this.par = par;
	}

	public WordData StringToWordData(string word) {
		if (stringWordDataDictionary.ContainsKey(word))
			return stringWordDataDictionary[word];
		WordData data = (Resources.Load(word) as WordData);
		if (data != null) {
			stringWordDataDictionary.Add(word, data);
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
		return Mathf.Clamp01((starAverage - par + 0.1f) / (5 - par + 0.1f)); //0.1f so that you get score at exactly spar
	}
}
