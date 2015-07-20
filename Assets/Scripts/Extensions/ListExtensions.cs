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

  public static IEnumerable<T> Sample<T>(this IList<T> list, int N) {
    var shuffled = new List<T>(list);
    shuffled.Shuffle();
    return shuffled.Take(N);
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

  public static IEnumerable<T> Sample<T>(this List<T> list, int N) {
    var shuffled = new List<T>(list);
    shuffled.Shuffle();
    return shuffled.Take(N);
  }
}
