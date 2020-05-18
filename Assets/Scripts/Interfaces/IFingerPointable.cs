using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFingerPointable {

	int GetOrder();
	UIButton GetPointedButton();
	UIButton[] GetAllButtons();

}
