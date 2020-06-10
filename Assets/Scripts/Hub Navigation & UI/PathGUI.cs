using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGUI : MonoBehaviour {

	Transform pathRoot;
	PathNodeGUI pathNode;
	
	RectTransform dummyNode;

	List<PathNodeGUI>[] pathNodes;
	Dictionary<Transform, List<PathNodeGUI>[]> rootNodeDictionary = new Dictionary<Transform, List<PathNodeGUI>[]>();

	bool waitingEndOfFrame;

	public void SetUp(Transform pathRoot, GameObject pathNode, int maxPathBatches) {
		if (pathRoot == null || pathNode == null)
			return;
		this.pathNode = pathNode.GetComponent<PathNodeGUI>();
		this.pathRoot = pathRoot;
		if (!rootNodeDictionary.ContainsKey(pathRoot)) {
			rootNodeDictionary.Add(pathRoot, new List<PathNodeGUI>[pathRoot.GetComponentsInChildren<RectTransform>().Length]);
			RectTransform[] nodes = new RectTransform[Mathf.Min(maxPathBatches, pathRoot.childCount)];
			for (int i = 0; i < nodes.Length; ++i) {
				nodes[i] = pathRoot.GetChild(i).GetComponent<RectTransform>();
			}
			dummyNode = Instantiate(pathNode.gameObject, pathRoot).transform.GetChild(0).GetComponent<RectTransform>(); //The child holds the proper aspect ratio since it has the image
			dummyNode.GetComponentInParent<PathNodeGUI>().Disable();
		    StartCoroutine(SetUpEndOfFrame(nodes)); // Dummy gets properly sized at end of frame, we can only take measurements then
		}
	}

	IEnumerator SetUpEndOfFrame(RectTransform[] nodes) {
		waitingEndOfFrame = true;
		yield return new WaitForEndOfFrame();
		pathNodes = rootNodeDictionary[pathRoot];
		int pathNodeAmount;
		Vector3 start, end;
		PathNodeGUI pathNode;
		int j = 0;
		for (int i = 0; i < nodes.Length - 1; ++i) {
			pathNodes[i] = new List<PathNodeGUI>();
			j = i + 1;
			start = Vector3.MoveTowards(nodes[i].transform.localPosition, nodes[j].transform.localPosition, nodes[i].rect.width / 2);
			end = Vector3.MoveTowards(nodes[j].transform.localPosition, nodes[i].transform.localPosition, nodes[j].rect.width / 2);
			pathNodeAmount = Mathf.Max(1, Mathf.RoundToInt(Vector3.Distance(start, end) / dummyNode.rect.width / 2f));
			for (int k = 0; k < pathNodeAmount; ++k) {
				pathNode = Instantiate(this.pathNode.gameObject.GetComponent<PathNodeGUI>(), pathRoot);
				pathNode.transform.localPosition = Vector3.Lerp(start, end, (k * 2 + 1) / (pathNodeAmount * 2.0f));
				pathNodes[i].Add(pathNode);
			}
		}
		Destroy(dummyNode.gameObject);
		waitingEndOfFrame = false;
	}

	public void SetLocked(int index, bool locked, bool nextLocked) {
		if (waitingEndOfFrame) {
			StartCoroutine(WaitLockSetting(index, locked, nextLocked));
			return;
		}
		if (pathNodes[index] == null)
			return;
		foreach (PathNodeGUI path in rootNodeDictionary[pathRoot][index])
			path.SetLocked(locked || nextLocked);
	}

	IEnumerator WaitLockSetting(int index, bool locked, bool nextLocked) {
		while (waitingEndOfFrame)
			yield return null;
		SetLocked(index, locked, nextLocked);
	}
}
