using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class SortedListExtensions {

  public static void AddRange<TKey, TValue>(this SortedList<TKey, TValue> list, IEnumerable<KeyValuePair<TKey, TValue>> collection) {
    foreach (var item in collection) {
      list.Add(item.Key, item.Value);
    }
  }
}
