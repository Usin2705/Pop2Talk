using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolable {
    void OnReturnToPool();
}

public class PoolMaster {

    static PoolMaster instance;
    public static PoolMaster Instance {
        get {
            if (instance == null) {
                instance = new PoolMaster();
            }

            return instance;
        }
    }

    Dictionary<int, System.Collections.Generic.HashSet<GameObject>> objectPools = new Dictionary<int, System.Collections.Generic.HashSet<GameObject>>();

    private Transform poolRoot;

    //To make sure poolable objects are disabled for atleast 1 frame, might cause issues with missed OnEnables otherwise
    private System.Collections.Generic.HashSet<GameObject> evenFrameReturnSet = new System.Collections.Generic.HashSet<GameObject>();
    private int evenFrame = 0;
    private System.Collections.Generic.HashSet<GameObject> oddFrameReturnSet = new System.Collections.Generic.HashSet<GameObject>();
    private int oddFrame = 0;

    public GameObject GetPooledObject(GameObject prefab) {
        UpdateFrameReturnSets(Time.frameCount);
        if (poolRoot == null) {
            CreatePoolRoot();
        }

        if (prefab.GetComponent<IPoolable>() == null) {
            Debug.LogError("PoolMaster is creating an object from prefab (" + prefab + ") that isn't poolable.");
            return GameObject.Instantiate(prefab);
        }

        int instanceId = prefab.GetInstanceID();

        System.Collections.Generic.HashSet<GameObject> set;
        if (!objectPools.ContainsKey(instanceId)) {
            set = new System.Collections.Generic.HashSet<GameObject>();
            objectPools.Add(instanceId, set);
        } else {
            set = objectPools[instanceId];
        }

        foreach (GameObject go in set) {
            if (go != null && !go.activeSelf && !FrameReturnSetContains(go, Time.frameCount)) {
                go.SetActive(true);
                go.transform.SetParent(null);
                return go;
            }
        }
        
        GameObject poolableObject = GameObject.Instantiate(prefab);
        poolableObject.SetActive(true);
        set.Add(poolableObject);

        return poolableObject;
    }

    void CreatePoolRoot() {
        poolRoot = new GameObject("Object pool").transform;
    }

    private void UpdateFrameReturnSets(int frameCount) {
        if (frameCount % 2 == 0) {
            if (frameCount != evenFrame) {
                evenFrameReturnSet.Clear();
            }
            oddFrameReturnSet.Clear();
            evenFrame = frameCount;
        } else {
            if (frameCount != oddFrame) {
                oddFrameReturnSet.Clear();
            }
            evenFrameReturnSet.Clear();
            oddFrame = frameCount;
        }
    }

    private void FrameReturnSetAdd(GameObject gameObject, int frameCount) {
        if (frameCount % 2 == 0) {
            evenFrameReturnSet.Add(gameObject);
        } else {
            oddFrameReturnSet.Add(gameObject);
        }
    }

    private bool FrameReturnSetContains(GameObject gameObject, int frameCount) {
        if (frameCount % 2 == 0) {
            return evenFrameReturnSet.Contains(gameObject);
        } else {
            return oddFrameReturnSet.Contains(gameObject);
        }
    }
    

    public void Destroy(GameObject gameObject) {
        UpdateFrameReturnSets(Time.frameCount);
        var objects = gameObject.GetComponents<IPoolable>();
        if (objects != null && objects.Length > 0) {
            if (poolRoot == null) {
                CreatePoolRoot();
            }
            gameObject.transform.SetParent(poolRoot);
            gameObject.SetActive(false);
            FrameReturnSetAdd(gameObject, Time.frameCount);
            foreach (var o in objects) {
                o.OnReturnToPool();
            }
        } else {
            Debug.LogError("PoolMaster is destroying an object ("+gameObject+") that isn't poolable.");
            GameObject.Destroy(gameObject);
        }
    }
}