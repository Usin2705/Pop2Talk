using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPageHandler : MonoBehaviour {

	[SerializeField] int itemPerGrid;
	[SerializeField] UIButton nextButton;
	[SerializeField] UIButton prevButton;
	[SerializeField] GameObject gridBlueprint;

	int currentCollection = 0;

	List<List<Transform>> pages = new List<List<Transform>>();
	List<int> currentPages = new List<int>();

   void Awake() {
		nextButton.SubscribePress(Next);
		prevButton.SubscribePress(Prev);
	}

	void OnEnable() {
		UpdateButtons();
	}

	public Transform GetNextParent(int collectionIndex) {
		while (collectionIndex >= pages.Count) {
			pages.Add(new List<Transform>());
			currentPages.Add(0);
		}
		List<Transform> collection = pages[collectionIndex]; 
		if (collection.Count == 0 || collection[collection.Count-1].childCount == itemPerGrid) {
			collection.Add(Instantiate(gridBlueprint, transform).transform);
		}
		if (collectionIndex == currentCollection) {
			collection[collection.Count - 1].gameObject.SetActive(currentPages[collectionIndex] == collection.Count - 1);
			UpdateButtons();
		}
		return collection[collection.Count - 1];
	}

	public void ShowCollection(int collectionIndex) {
		pages[currentCollection][currentPages[currentCollection]].gameObject.SetActive(false);
		currentCollection = collectionIndex;
		pages[currentCollection][currentPages[currentCollection]].gameObject.SetActive(true);
		UpdateButtons();
	}

	void Next() {
		ChangePage(1);
	}

	void Prev() {
		ChangePage(-1);
	}

	void ChangePage(int change) {
		pages[currentCollection][currentPages[currentCollection]].gameObject.SetActive(false);
		currentPages[currentCollection] += change;
		pages[currentCollection][currentPages[currentCollection]].gameObject.SetActive(true);
		UpdateButtons();
	}

	void UpdateButtons() {
		nextButton.gameObject.SetActive(currentPages[currentCollection] != pages[currentCollection].Count - 1);
		prevButton.gameObject.SetActive(currentPages[currentCollection] != 0);
	}
}
