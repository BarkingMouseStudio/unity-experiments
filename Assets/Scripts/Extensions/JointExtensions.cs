using UnityEngine;

public static class JointExtensions {

  public static Vector3 GetWorldAnchor(this Joint joint) {
    return joint.transform.TransformPoint(joint.anchor);
  }

  public static Vector3 GetWorldAxis(this Joint joint) {
    return joint.transform.TransformDirection(joint.axis);
  }
}
