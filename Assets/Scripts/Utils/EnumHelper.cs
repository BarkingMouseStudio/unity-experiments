using UnityEngine;

public static class EnumHelper {

  public static T Random<T>() {
    var values = System.Enum.GetValues(typeof(T));
    return (T)values.GetValue(UnityEngine.Random.Range(0, values.Length));
  }
}
