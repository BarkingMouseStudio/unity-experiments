using UnityEngine;

public static class RandomHelper {

  public static bool NextBool() {
    return Random.value > 0.5f;
  }

  public static float NextGaussian() {
    return Mathf.Sqrt(-2.0f * Mathf.Log(Random.value)) *
           Mathf.Sin(2.0f * Mathf.PI * Random.value);
  }

  public static float NextCauchy(float m, float gamma) {
    var a = NextGaussian();
    var b = NextGaussian();
    return m + gamma * 0.01f * (a / b);
  }
}
