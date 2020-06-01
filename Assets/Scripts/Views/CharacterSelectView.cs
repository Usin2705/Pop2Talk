using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectView : View {

	[SerializeField] View planetHub;
	[SerializeField] View tutorial;
	//[SerializeField] View login;
	[Space]
	[SerializeField] GameObject characterButtonPrefab;
	[SerializeField] UIButton backButton;
	[SerializeField] UIButton nextButton;
	[SerializeField] Transform buttonRoot;
	[SerializeField] SpeechCollection characterSelected;


	List<UIButton> buttons;
	int chosenIndex = -1;
	int prevChosenIndex = -1;

	protected override void Initialize() {
		base.Initialize();

		Sprite[] sprites = CharacterManager.GetManager().GetCharacterSprites();
		UIButton button;
		backButton.SubscribePress(Exit);
		nextButton.SubscribePress(NextPressed);
		buttons = new List<UIButton>();
		
		for (int i = 0; i < sprites.Length; ++i) {
			button = Instantiate(characterButtonPrefab, buttonRoot).GetComponent<UIButton>();
			button.SetSprite(sprites[i]);
			int j = i; // If subscribed simply to i, the lambda apparentaly keeps a reference to it and makes all subscriptions equal to sprites.Length
			button.SubscribePress(() => { ButtonPressed(j); });
			buttons.Add(button);
		}
	}

	void ButtonPressed(int index) {
		if (chosenIndex != -1)
			buttons[chosenIndex].Deselect();
		if (chosenIndex == index)
			chosenIndex = -1;
		else
			chosenIndex = index;

		nextButton.gameObject.SetActive(chosenIndex != -1);

	}

	void NextPressed() {
		CharacterManager.GetManager().SetCharacter(chosenIndex);
		if (CharacterManager.GetManager().CurrentCharacter != null) {
			doExitFluff = chosenIndex != prevChosenIndex;
			prevChosenIndex = chosenIndex;
			ViewManager.GetManager().ShowView(planetHub);
		}
	}

	public override void ExitFluff(Callback Done) {
		NetworkManager.GetManager().CharacterSelectEvent("character_selection", CharacterManager.GetManager().CurrentCharacter.name);
		CharacterManager.GetManager().ShowCharacter(characterSelected, sortingOrder, () => { base.ExitFluff(Done); });
	}

	/*void Back() {
		doExitFluff = false;
		ViewManager.GetManager().ShowView(login);
	}*/

	void Exit() {
		NetworkManager.GetManager().ControlledExit();
		Application.Quit();
	}

	public override UIButton GetPointedButton() {
        if(chosenIndex != -1)
        {
            return nextButton;
        }
		return buttons.GetRandom();
	}

	public override UIButton[] GetAllButtons() {

        UIButton[] uiButtons = new UIButton[buttons.Count + 2];
        for (int i = 0; i < buttons.Count; ++i)
            uiButtons[i] = buttons[i];
        uiButtons[uiButtons.Length - 1] = backButton;
        uiButtons[uiButtons.Length - 2] = nextButton;
        return uiButtons;
    }
}
