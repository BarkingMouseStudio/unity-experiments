using UnityEngine;

public static class DoubleExtensions {

  public static bool IsFinite(this double v) {
    return !double.IsNaN(v) && !double.IsInfinity(v);
  }
}
