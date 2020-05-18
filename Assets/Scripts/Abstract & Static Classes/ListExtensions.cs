using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class ListExtensions {

    public static void Shuffle<T>(this IList<T> list) {
        Shuffle(list, list.Count);
    }

    public static void Shuffle<T>(this IList<T> list, int maxIndexToShuffle) {
        maxIndexToShuffle = Mathf.Clamp(maxIndexToShuffle, 0, list.Count);
        while (maxIndexToShuffle > 1) {
            maxIndexToShuffle--;
            int k = Random.Range(0, maxIndexToShuffle + 1);
            T value = list[k];
            list[k] = list[maxIndexToShuffle];
            list[maxIndexToShuffle] = value;
        }
    }

    public static T GetRandom<T>(this IList<T> list) {
		if (list == null || list.Count == 0)
			return default(T);
        return list[Random.Range(0, list.Count)];
    }
}
