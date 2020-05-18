using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable {
    #region IComparer<TKey> Members

    public int Compare(TKey x, TKey y) {
        int result = x.CompareTo(y);

        if (result == 0)
            return 1;
        else
            return result;
    }

    #endregion
}
