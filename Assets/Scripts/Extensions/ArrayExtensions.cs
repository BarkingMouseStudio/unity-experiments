using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class ArrayExtensions {

  public static void Shuffle<T>(this T[] arr) {
    for (int i = 0; i < arr.Length; i++) {
      arr.Swap(i, UnityEngine.Random.Range(i, arr.Length));
    }
  }

  public static void Swap<T>(this T[] arr, int i, int j) {
    var temp = arr[i];
    arr[i] = arr[j];
    arr[j] = temp;
  }

  public static void Fill<T>(this T[] arr, T val) {
    for (int i = 0; i < arr.Length; i++) {
      arr[i] = val;
    }
  }
}
