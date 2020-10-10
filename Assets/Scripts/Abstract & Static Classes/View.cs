using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class View : MonoBehaviour, IFingerPointable {

	protected bool doExitFluff;
	protected bool initialized;
	protected int sortingOrder;

	public bool DoExitFluff { get { return doExitFluff; } }

	public virtual void Activate() {
		gameObject.SetActive(true);
		if (!initialized)
			Initialize();
	}

	public virtual void Deactivate() {
		gameObject.SetActive(false);
	}

	protected virtual void Initialize() {
		initialized = true;
		try {
			sortingOrder = GetComponent<Canvas>().sortingOrder;
		}
		catch (System.Exception e){
			DebugMaster.Instance.DebugText("Sorting order: " + e);
		}
	}

	public virtual void ExitFluff(Callback Done) {
		Done();
	}

	public virtual void EnterFluff(Callback Done) {
		Done();
	}

	public int GetOrder() {
		return sortingOrder;
	}

	public virtual void Back() {

	}

	public abstract UIButton GetPointedButton();
	public abstract UIButton[] GetAllButtons();
}
