using System;
using System.Collections;
using System.Collections.Generic;

public static class SliceExtensions {

  public static int FindMaxIndex<T>(this Slice<T> source) where T : IComparable<T> {
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
