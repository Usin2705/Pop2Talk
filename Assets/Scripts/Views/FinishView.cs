using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinishView : View {

	[SerializeField] View shipHub;
	[SerializeField] View gridGame;
	[Space]
	[SerializeField] UIButton nextButton;
	[SerializeField] UIButton prevButton;
	[Space]
	[SerializeField] Text moonText;


	protected override void Initialize() {
		base.Initialize();
		nextButton.SubscribePress(GotoShipHub);
		prevButton.SubscribePress(GotoGridGame);
		prevButton.gameObject.SetActive(false);
	}

	public override void Activate() {
		base.Activate();
		MakeCoins();
	}

	void MakeCoins() {
		int increase = CurrencyMaster.Instance.IncreaseCoins(WordMaster.Instance.GetStarRatio(WordMaster.Instance.TotalStars), 
			GridGameMaster.Instance.GetDustRatio(GridGameMaster.Instance.SpaceDust));
		if (increase == 0) { 
			prevButton.gameObject.SetActive(true);
		}
		moonText.text = increase.ToString();
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
