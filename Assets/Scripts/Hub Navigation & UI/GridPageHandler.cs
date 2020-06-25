using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPageHandler : MonoBehaviour {

	[SerializeField] int itemPerGrid;
	[SerializeField] UIButton nextButton;
	[SerializeField] UIButton prevButton;
	[SerializeField] GameObject gridBlueprint;

	int currentPage = 0;

	void Awake() {
		nextButton.SubscribePress(Next);
		prevButton.SubscribePress(Prev);
	}

	void OnEnable() {
		UpdateButtons();
	}

	public Transform GetParent() {
		if (transform.childCount == 0 || transform.GetChild(transform.childCount-1).childCount == itemPerGrid) {
			Instantiate(gridBlueprint, transform);
		}
		transform.GetChild(transform.childCount - 1).gameObject.SetActive(currentPage == transform.childCount - 1);
		UpdateButtons();
		return transform.GetChild(transform.childCount - 1);
	}

	void Next() {
		transform.GetChild(currentPage).gameObject.SetActive(false);
		currentPage++;
		transform.GetChild(currentPage).gameObject.SetActive(true);
		UpdateButtons();
	}

	void Prev() { 
		transform.GetChild(currentPage).gameObject.SetActive(false);
		currentPage--;
		transform.GetChild(currentPage).gameObject.SetActive(true);
		UpdateButtons();
	}

	void UpdateButtons() {
		nextButton.gameObject.SetActive(currentPage != transform.childCount - 1);
		prevButton.gameObject.SetActive(currentPage != 0);
	}
}
