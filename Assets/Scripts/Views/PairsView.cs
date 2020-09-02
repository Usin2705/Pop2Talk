using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PairsView : View {

	[SerializeField] View shipHub;
	[SerializeField] GameObject pairPrefab;
	[SerializeField] Sprite backSprite;
	[SerializeField] float shufflespeed;

	WordData[] words;

	Dictionary<PairCard, WordData> pairs = new Dictionary<PairCard, WordData>();

	protected override void Initialize() {
		base.Initialize();
	}

	public override void Activate() {
		base.Activate();
	}

	void PrepareCards() {
		Vector3 startPos = Vector3.zero;
		Transform cardRoot = null;
		PairCard pairCard;
		List<PairCard> pairCards = new List<PairCard>();
		for (int i = 0; i < words.Length; ++i) {
			for (int j = 0; i < 2; ++j) {
				pairCard = Instantiate(pairPrefab, cardRoot).GetComponent<PairCard>();
				pairCard.transform.position = startPos;
				pairCard.SetSprites(words[i].picture, backSprite);
				pairs.Add(pairCard, words[i]);
			}
		}
		StartCoroutine(ArrangePairCards(pairCards));
	}

	IEnumerator ArrangePairCards(List<PairCard> pairCards) {
		pairCards.Shuffle();
		Vector3[] positions = GetCardPositions(pairCards.Count);

		yield return null;
	}

	Vector3[] GetCardPositions(int count) {
		return null;
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
}
