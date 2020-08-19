using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPageHandler : MonoBehaviour {

	[SerializeField] int itemPerGrid;
	[SerializeField] UIButton nextButton;
	[SerializeField] UIButton prevButton;
	[SerializeField] GameObject gridBlueprint;

	int currentCollectionIndex = 0;

	List<List<Transform>> collections = new List<List<Transform>>();
	List<int> currentPages = new List<int>();

	void Awake() {
		nextButton.SubscribePress(Next);
		prevButton.SubscribePress(Prev);
	}

	void OnEnable() {
		UpdateButtons();
	}

	public Transform GetNextParent(int collectionIndex) {
		while (collectionIndex >= collections.Count) {
			collections.Add(new List<Transform>());
			currentPages.Add(0);
		}
		List<Transform> collection = collections[collectionIndex];
		if (collection.Count == 0 || collection[collection.Count - 1].childCount == itemPerGrid) {
			collection.Add(Instantiate(gridBlueprint, transform).transform);
			collection[collection.Count - 1].gameObject.SetActive(collectionIndex == currentCollectionIndex);
		}
		if (collectionIndex == currentCollectionIndex) {
			collection[collection.Count - 1].gameObject.SetActive(currentPages[collectionIndex] == collection.Count - 1);
			UpdateButtons();
		}
		return collection[collection.Count - 1];
	}

	public void ShowCollection(int collectionIndex) {
		while (collectionIndex >= collections.Count) {
			collections.Add(new List<Transform>());
			currentPages.Add(0);
		}
		if (currentPages[currentCollectionIndex] < collections[currentCollectionIndex].Count)
			collections[currentCollectionIndex][currentPages[currentCollectionIndex]].gameObject.SetActive(false);
		currentCollectionIndex = collectionIndex;
		if (currentPages[currentCollectionIndex] < collections[currentCollectionIndex].Count)
			collections[currentCollectionIndex][currentPages[currentCollectionIndex]].gameObject.SetActive(true);
		UpdateButtons();
	}

	void Next() {
		ChangePage(1);
	}

	void Prev() {
		ChangePage(-1);
	}

	void ChangePage(int change) {
		collections[currentCollectionIndex][currentPages[currentCollectionIndex]].gameObject.SetActive(false);
		currentPages[currentCollectionIndex] += change;
		collections[currentCollectionIndex][currentPages[currentCollectionIndex]].gameObject.SetActive(true);
		UpdateButtons();
	}

	void UpdateButtons() {
		while (currentCollectionIndex >= collections.Count) {
			collections.Add(new List<Transform>());
			currentPages.Add(0);
		}
		nextButton.gameObject.SetActive(currentPages[currentCollectionIndex] < collections[currentCollectionIndex].Count - 1);
		prevButton.gameObject.SetActive(currentPages[currentCollectionIndex] != 0);
	}
}
