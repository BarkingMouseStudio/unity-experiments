using UnityEngine;

public static class Vector3Extensions {

  public static bool IsFinite(this Vector3 v) {
    return v.sqrMagnitude.IsFinite();
  }
}
