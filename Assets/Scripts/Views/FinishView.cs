﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinishView : View {

	[SerializeField] View shipHub = null;
	[SerializeField] View gridGame = null;
	[Space]
	[SerializeField] GameObject visualScreen = null;
	[SerializeField] GameObject coinScreen = null;
	[Space]
	[SerializeField] UIButton nextButton = null;
	[SerializeField] UIButton prevButton = null;
	[Space]
	[SerializeField] Text coinText = null;
	[SerializeField] Text starText = null;
	[SerializeField] Text dustText = null;
	[SerializeField] FinishStar finishStar = null;
	[SerializeField] Image whiteCurtain = null;

	protected override void Initialize() {
		base.Initialize();
		nextButton.SubscribePress(GotoShipHub);
		prevButton.SubscribePress(GotoGridGame);
	}

	public override void Activate() {
		base.Activate();
		if (DebugMaster.Instance.skipTransitions)
			MakeCoins();
		else
			StartCoroutine(CoinVisual());
	}

	void MakeCoins() {
		int increase = CurrencyMaster.Instance.IncreaseCoins(WordMaster.Instance.GetStarRatio(WordMaster.Instance.TotalStars / (float)WordMaster.Instance.MaxCards),
			GameMaster.Instance.GetDustRatio(GameMaster.Instance.SpaceDust));
		if (increase == 0) {
			prevButton.gameObject.SetActive(true);
			NetworkManager.GetManager().LevelCompleted(GameMaster.Instance.CurrentLevel.name, false, false);
		} else {
			prevButton.gameObject.SetActive(false);
			GameMaster.Instance.CompleteCount++;
			NetworkManager.GetManager().LevelCompleted(GameMaster.Instance.CurrentLevel.name, true, false);
		}
		coinText.text = increase.ToString();
	}

	IEnumerator CoinVisual() {
		InputManager.GetManager().SendingInputs = false;
		visualScreen.SetActive(true);
		coinScreen.SetActive(false);
		starText.text = WordMaster.Instance.TotalStars.ToString();
		dustText.text = GameMaster.Instance.SpaceDust.ToString();
		yield return new WaitForSeconds(1);
		float a = 0;
		float speed = 180;
		if (WordMaster.Instance.TotalStars > 0) {
			List<FinishStar> stars = new List<FinishStar>();
			if (WordMaster.Instance.TotalStars > 1) {
				float targetAngle = 360 - 360f / WordMaster.Instance.TotalStars;
				float collector = 0;
				while (a < targetAngle) {
					if (collector <= 0) {
						collector = 360f / WordMaster.Instance.TotalStars;
						stars.Add(Instantiate(finishStar.gameObject, finishStar.transform.parent).GetComponent<FinishStar>());
						starText.text = (WordMaster.Instance.TotalStars - stars.Count).ToString();
					} else
						collector -= speed * Time.deltaTime;
					a += speed * Time.deltaTime;
					foreach (FinishStar fs in stars) {
						fs.angle += speed * Time.deltaTime;
					}
					yield return null;
				}
			}
			stars.Add(finishStar);
			starText.text = "0";
			a = 0;
			while (a < 1) {
				a += Time.deltaTime / 2;
				foreach (FinishStar fs in stars) {
					fs.ratio = Mathf.Lerp(1f, 0.1f, Mathf.Pow(Mathf.Max(0, (a - 0.5f) * 2), 2));
					fs.angle += speed * Time.deltaTime * Mathf.Lerp(1, 2.5f, a * 2)/fs.ratio;
				}
				yield return null;
			}
			for (int i = 0; i < stars.Count - 1; ++i) {
				Destroy(stars[i].gameObject);
			}
			finishStar.angle = 0;
			finishStar.ratio = 1;
		}
		whiteCurtain.gameObject.SetActive(true);
		MakeCoins();
		coinScreen.gameObject.SetActive(true);
		visualScreen.gameObject.SetActive(false);
		yield return new WaitForSeconds(0.5f);
		a = 0;
		while (a < 1) {
			a += Time.deltaTime / 0.5f;
			whiteCurtain.color = new Color(1, 1, 1, 1 - a);
			yield return null;
		}
		whiteCurtain.gameObject.SetActive(false);
		whiteCurtain.color = Color.white;
		InputManager.GetManager().SendingInputs = true;
	}

	void GotoShipHub() {
		ViewManager.GetManager().ShowView(shipHub);
	}

	void GotoGridGame() {
		ViewManager.GetManager().ShowView(gridGame);
	}

	public override UIButton GetPointedButton() {
		return nextButton;
	}

	public override UIButton[] GetAllButtons() {
		return new UIButton[] { nextButton, prevButton };
	}
}
