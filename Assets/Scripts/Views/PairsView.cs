using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PairsView : View, IMinigame {

	[SerializeField] View shipHub;
	[SerializeField] GameObject pairPrefab;
	[SerializeField] Sprite backSprite;
	[SerializeField] float moveSpeed;
	[SerializeField] Vector3 scale;
	[SerializeField] Vector3 center;
	[Space]
	[SerializeField] UIButton backButton;

	int maxPairs = 6;

	List<WordData> words;
	Dictionary<FlipCard, WordData> pairs = new Dictionary<FlipCard, WordData>();
	int movingCards;
	int correctCards;
	
	FlipCard cardOne;
	FlipCard cardTwo;

	protected override void Initialize() {
		base.Initialize();
		backButton.SubscribePress(GotoShipHub);
	}

	public override void Activate() {
		base.Activate();
		string[] samples = WordMaster.Instance.GetSampleWords();
		Debug.Log("Samples length: " + samples.Length);
		words = new List<WordData>();
		WordData data;
		int pairs = Mathf.Min(maxPairs, samples.Length);
		for (int i = 0; i < pairs; ++i) {
			data = WordMaster.Instance.StringToWordData(samples[i]);
			if (data != null)
				words.Add(data);
		}
		PrepareCards();
	}

	void PrepareCards() {
		Transform cardRoot = null;
		InputManager.GetManager().SendingInputs = false;
		Vector3[] positions = FlipCard.GetCardPositions(center, words.Count * 2);
		positions.Shuffle();
		movingCards = positions.Length;
		cardOne = null;
		cardTwo = null;
		for (int i = 0; i < words.Count; ++i) {
			for (int j = 0; j < 2; ++j) {
				FlipCard pairCard = Instantiate(pairPrefab, cardRoot).GetComponent<FlipCard>(); //nonoptimized to get around lambda weirdness
				pairCard.transform.position = center;
				pairCard.transform.localScale = scale;
				pairCard.SetSprites(words[i].picture, backSprite);
				pairCard.SetClick(() => { CardClicked(pairCard); });
				pairCard.Move(positions[i * 2 + j], moveSpeed, 1f, CardTargetReached);
				pairs.Add(pairCard, words[i]);
			}
		}
		if (words.Count == 0)
			ShowCoins();
	}

	void CardClicked(FlipCard card) {
		if (cardOne != null && cardTwo != null)
			return;
		AudioMaster.Instance.Play(this, AudioMaster.Instance.InstancifyClip(pairs[card].pronunciations.GetRandom()));
		if (cardOne == null)
			cardOne = card;
		else
			cardTwo = card;
		card.Rotate(true, CheckPair);
	}

	void CheckPair() {
		if (cardOne == null || cardTwo == null || cardOne.Rotating || cardTwo.Rotating)
			return;
		StartCoroutine(CheckPairDelay());
	}

	IEnumerator CheckPairDelay() {
		if (pairs[cardOne] == pairs[cardTwo]) {
			correctCards += 2;
			if (correctCards == pairs.Count) {
				yield return new WaitForSeconds(0.5f);
				ShowCoins();
			}
			cardOne = null;
			cardTwo = null;
		} else {
			yield return new WaitForSeconds(0.5f);
			cardOne.Rotate(false, () => { cardOne = null; });
			cardTwo.Rotate(false, () => { cardTwo = null; });
		}
	}

	void ShowCoins() {
		int coins = CurrencyMaster.Instance.GetLootLevelCoins(CurrencyMaster.Instance.LootLevel);
		CurrencyMaster.Instance.ModifyCoins(coins);
		GameMaster.Instance.CompleteCount = 0;
		UnlockOverlay.Instance.ShowUnlock(sortingOrder, IconManager.GetManager().coinIcon, coins.ToString(), GotoShipHub);
	}

	void CardTargetReached() {
		movingCards--;
		if (movingCards == 0) {
			InputManager.GetManager().SendingInputs = true;
		}
	}

	public override void Deactivate() {
		base.Deactivate();
		foreach (FlipCard pc in pairs.Keys) {
			PoolMaster.Instance.Destroy(pc.gameObject);
		}
	}

	void GotoShipHub() {
		ViewManager.GetManager().ShowView(shipHub);
	}

	public override UIButton GetPointedButton() {
		return null;
	}

	public override UIButton[] GetAllButtons() {
		return null;
	}

	public Sprite GetIcon() {
		return backSprite;
	}
}
