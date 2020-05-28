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

	public int CardsRemaining {
		get {
			return (cardQueue == null) ? 0 : cardQueue.Count + forcedQueue.Count + forcedBatch.Count + nextWordBatch.Count;
		}
	}

	List<WordData> nextWordBatch = new List<WordData>();
	List<WordCardType> nextWordTypeBatch = new List<WordCardType>();

	public int RemainingBatch {
		get {
			return (forcedBatch == null) ? nextWordBatch.Count : forcedBatch.Count + nextWordBatch.Count;
		}
	}

	Queue<WordCardType> cardQueue;
	Queue<string> firstTimeWords;
	Queue<bool> adaptionQueue;
	Dictionary<WordCardType, Dictionary<string, int>> showTimes;
	Dictionary<string, string> roveDictionary = new Dictionary<string, string>();
	Dictionary<string, WordData> stringWordDataDictionary = new Dictionary<string, WordData>();
	int newQuizzes = 0;
	int adaptionSkips = 0;
	Queue<WordData> forcedQueue = new Queue<WordData>();
	List<WordData> forcedBatch = new List<WordData>();

	float roveSwitchPercentage = 0.4f;

	WordListSettings[] wordLists;
	WordData[] forcedWords;
	int totalMaxCards;
	int quizMaxCards;
	int adaptiveWordsMax;
	int batchSize;

	BaseWordCardHandler currentWordCardHandler;
	Dictionary<WordCardType, BaseWordCardHandler> wordCardHandlers = new Dictionary<WordCardType, BaseWordCardHandler>();

	private void Awake() {
		showTimes = new Dictionary<WordCardType, Dictionary<string, int>>();
		showTimes.Add(WordCardType.Rehearse, new Dictionary<string, int>());
		showTimes.Add(WordCardType.Quiz, new Dictionary<string, int>());
		//showTimes.Add(WordCardType.Memory, new Dictionary<string, int>());
	}

	public void SetWords(WordListSettings[] wordLists, int totalCards, int quizCards, int adaptiveWords, int batchSize, WordData[] forcedWords) {
		this.wordLists = wordLists;
		totalMaxCards = totalCards;
		if (forcedWords != null)
			totalMaxCards -= forcedWords.Length;
		quizMaxCards = quizCards;
		adaptiveWordsMax = adaptiveWords;
		this.batchSize = batchSize;
		this.forcedWords = forcedWords;
	}

	public void Initialize() {
		List<string> toBeFirstQueue = new List<string>();
		List<string> toBeRoveDictionary = new List<string>();
		int firsts = 0;
		newQuizzes = 0;
		adaptionSkips = 0;
		foreach (WordListSettings wls in wordLists) {
			if (wls.first) {
				foreach (string s in wls.wordList.GetWords()) {
					toBeFirstQueue.Add(s);
					firsts++;
					if (wls.rove)
						toBeRoveDictionary.Add(s);
				}
			} else {
				foreach (string s in wls.wordList.GetWords()) {
					foreach (WordCardType type in showTimes.Keys)
						if (!showTimes[type].ContainsKey(s))
							showTimes[type].Add(s, 0);

					if (wls.rove)
						toBeRoveDictionary.Add(s);
				}
			}
		}
		toBeFirstQueue.Shuffle();
		firstTimeWords = new Queue<string>(toBeFirstQueue);
		List<WordCardType> toBeTotalCards = new List<WordCardType>();
		firsts = Mathf.Clamp(firsts, 0, totalMaxCards - quizMaxCards);
		for (int i = 1; i < totalMaxCards - 1; ++i) {
			toBeTotalCards.Add((i < totalMaxCards - quizMaxCards) ? WordCardType.Rehearse : WordCardType.Quiz);
		}
		toBeRoveDictionary.Shuffle();
		roveDictionary.Clear();
		for (int i = 0; i < toBeRoveDictionary.Count; ++i) {
			roveDictionary.Add(toBeRoveDictionary[i], toBeRoveDictionary[(i == toBeRoveDictionary.Count - 1) ? 0 : i + 1]);
		}
		toBeTotalCards.Shuffle();
		if (totalMaxCards > 0) {
			if (firsts > 0)
				toBeTotalCards.Insert(0, WordCardType.Rehearse);
			else if (totalMaxCards > quizMaxCards)
				toBeTotalCards.Insert(0, WordCardType.Rehearse);
			else
				toBeTotalCards.Insert(0, WordCardType.Quiz);// makes first card nonquiz if possible
			if (totalMaxCards > 1)
				toBeTotalCards.Add((quizMaxCards > 0) ? WordCardType.Quiz : WordCardType.Rehearse); // makes last card quiz if possible
		}
		cardQueue = new Queue<WordCardType>(toBeTotalCards);
		List<bool> toBeAdaptionQueue = new List<bool>();
		for (int i = 0; i < cardQueue.Count - 1; ++i) {
			toBeAdaptionQueue.Add(i < adaptiveWordsMax);
		}
		toBeAdaptionQueue.Shuffle();
		toBeAdaptionQueue.Insert(0, adaptiveWordsMax == cardQueue.Count); // makes first word not adaptive if possible
		adaptionQueue = new Queue<bool>(toBeAdaptionQueue);
		forcedQueue = new Queue<WordData>(forcedWords);
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

	public void ChooseNextWordBatch() {
		nextWordBatch.Clear();
		nextWordTypeBatch.Clear();
		forcedBatch.Clear();
		if (forcedQueue.Count < batchSize) {
			for (int i = 0; i < Mathf.Min(batchSize-forcedQueue.Count, cardQueue.Count); ++i) {
				nextWordTypeBatch.Add(DequeueNextType());
				nextWordBatch.Add(GetWordForType(nextWordTypeBatch[nextWordTypeBatch.Count - 1]));
			}
		}
		while (forcedBatch.Count < (batchSize - nextWordBatch.Count) && forcedQueue.Count > 0)
			forcedBatch.Add(forcedQueue.Dequeue());
	}

	WordCardType DequeueNextType() {
		return cardQueue.Dequeue();
	}

	WordData GetWordForType(WordCardType type) {
		if (firstTimeWords != null && firstTimeWords.Count > 0) {
			string s = firstTimeWords.Dequeue();
			if (!showTimes[WordCardType.Quiz].ContainsKey(s)) {
				showTimes[WordCardType.Quiz].Add(s, -1); //-1 to ensure new words are quizzed first;
				newQuizzes++;
			}
			foreach (WordCardType wt in showTimes.Keys)
				if (!showTimes[wt].ContainsKey(s))
					showTimes[wt].Add(s, 0);
			return StringToWordData(s);
		} else {
			if (type != WordCardType.Quiz || newQuizzes == 0) { 
				adaptionSkips++;
				return GetRandomLeastShownWord(type);
			}
			while (adaptionSkips > 0 && adaptionQueue.Count > 0 && !adaptionQueue.Peek()) {
				adaptionQueue.Dequeue();
				adaptionSkips--;
			}

			if (adaptionQueue.Count > 0 && adaptionQueue.Dequeue()) {
				return GetAdaptiveWord();
			}
			return GetRandomLeastShownWord(type);
		}
	}

	string prevWord;

    WordData GetRandomLeastShownWord(WordCardType type) {
        Dictionary<string, int> dictionary = showTimes[type];
        List<string> leastShowns = new List<string>();
        int least = int.MaxValue;
        foreach (string s in dictionary.Keys) {
			if (prevWord == s)
				continue;
            if (dictionary[s] < least) {
                least = dictionary[s];
                leastShowns.Clear();
            }
            if (dictionary[s] == least)
                leastShowns.Add(s);
        }
        if (leastShowns.Count == 0) {
            Debug.LogError("No data in the worddata dictionary");
            return null;
        }
		if (least == -1)
			newQuizzes--;
        string chosen = leastShowns[Random.Range(0, leastShowns.Count)];
        dictionary[chosen]++;
		if (showTimes.Count > 1)
			prevWord = chosen;
        return StringToWordData(chosen);
    }
	
	public WordData UseNextWord() {
		if (forcedBatch.Count != 0) {
			WordData wd = forcedBatch[0];
			forcedBatch.RemoveAt(0);
			return wd;
		}
		if (nextWordBatch.Count == 0)
			return null;

		WordData word = nextWordBatch[0];
		nextWordBatch.RemoveAt(0);
		return word;
	}

	public WordCardType UseNextType() {
		if (forcedBatch.Count != 0)
			return WordCardType.Rehearse;
		WordCardType type = nextWordTypeBatch[0];
		nextWordTypeBatch.RemoveAt(0);
		return type;
	}

	public WordCardType PeekNextType() {
		if (forcedBatch.Count != 0)
			return WordCardType.Rehearse;
		return nextWordTypeBatch[0];
	}

	public WordData GetPopWord(float remainingPercentage) {
		if (nextWordBatch.Count == 0 && forcedBatch.Count == 0)
			return null;
		int random = Random.Range(0, forcedBatch.Count + nextWordBatch.Count);
		if (forcedBatch.Count > random)
			return forcedBatch.GetRandom();
		WordData word = nextWordBatch.GetRandom();
		if (remainingPercentage > roveSwitchPercentage) {
			if (roveDictionary.ContainsKey(word.name))
				return StringToWordData(roveDictionary[word.name]);
		}
		return word;
    }

	

	public void ShowWordCard(WordCardType type, string levelName, WordData word, Language targetLanguage, Language nativeLanguage, int order, IntCallback Done) {
		if (!wordCardHandlers.ContainsKey(type)) {
			switch (type) {
				case WordCardType.Rehearse:
					wordCardHandlers.Add(type, new RehearseCardHandler());
					break;
				case WordCardType.Quiz:
					wordCardHandlers.Add(type, new QuizCardHandler());
					break;
				case WordCardType.Memory:
					wordCardHandlers.Add(type, new MemoryCardHandler());
					break;
			}
		}
		wordCardHandlers[type].ShowCard(word, levelName, targetLanguage, nativeLanguage, order, Done);
	}

	#region Adaptation
	int maxRecordDepth = 100; //The search for adaptation is O(n^m) where n is the number of words and m is the record depth. Going infinite might pose problems

	Dictionary<string, List<int>> starRecords = new Dictionary<string, List<int>>();
	Dictionary<string, int> bestResults = new Dictionary<string, int>();

	WordData GetAdaptiveWord() {
		return StringToWordData(AdaptionSearchRecursion(starRecords.Keys));
	}

	string AdaptionSearchRecursion(ICollection<string> words, int currentDepth = 1) {
		if (words.Count == 0)
			return null;
		List<string> worstWords = new List<string>();
		int worstStars = ConstantHolder.maxStars, i;
		bool reachedLast = false;
		foreach(string s in words) {
			i = starRecords[s][starRecords[s].Count-currentDepth]; //Count from newest prononuncation, going in reverse order so no need to fiddle with the order
			if (i < worstStars) {
				worstWords.Clear();
				worstStars = i;
			}
			if (i == worstStars) {
				if (starRecords[s].Count == currentDepth) {
					if (!reachedLast) {
						worstWords.Clear();
						reachedLast = true;
					}
					worstWords.Add(s);
				} else if (!reachedLast) {
					worstWords.Add(s);
				}
			}
		}
		if (reachedLast)
			return worstWords.GetRandom();
		else
			return AdaptionSearchRecursion(worstWords, ++currentDepth);
	}

	public void RecordStarAmount(string word, int stars) {
		if (!starRecords.ContainsKey(word)) {
			starRecords.Add(word, new List<int>());
		}
		if (!bestResults.ContainsKey(word)) {
			bestResults.Add(word, stars);
		} else {
			if (bestResults[word] < stars)
				bestResults[word] = stars;
		}

		starRecords[word].Add(stars);
		if (starRecords[word].Count > maxRecordDepth)
			starRecords[word].RemoveAt(0);
	}

	public Dictionary<string, int> GetBestResults() {
		return bestResults;
	}

	#endregion
}
