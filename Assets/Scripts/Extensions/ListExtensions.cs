using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class IListExtensions {

  public static void Shuffle<T>(this IList<T> list) {
    for (int i = 0; i < list.Count; i++) {
      list.Swap(i, UnityEngine.Random.Range(i, list.Count));
    }
  }

  public static void Swap<T>(this IList<T> list, int i, int j) {
    var temp = list[i];
    list[i] = list[j];
    list[j] = temp;
  }

  public static int FindMaxIndex<T>(this IList<T> source) where T : IComparable<T> {
    var maxIndex = -1;
    if (source.Count > 0) {
      var max = source[0];
      maxIndex = 0;
      for (int i = 1; i < source.Count; i++) {
        if (source[i].CompareTo(max) > 0) {
          maxIndex = i;
        }
      }
    }
    return maxIndex;
  }
}

public static class ListExtensions {

  public static void Shuffle<T>(this List<T> list) {
    for (int i = 0; i < list.Count; i++) {
      list.Swap(i, UnityEngine.Random.Range(i, list.Count));
    }
  }

  public static void Swap<T>(this List<T> list, int i, int j) {
    var temp = list[i];
    list[i] = list[j];
    list[j] = temp;
  }
}
