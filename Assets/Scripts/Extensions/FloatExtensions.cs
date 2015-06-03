using UnityEngine;

public static class FloatExtensions {

  public static bool IsFinite(this float v) {
    return !float.IsNaN(v) && !float.IsInfinity(v);
  }
}
