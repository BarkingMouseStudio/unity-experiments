using UnityEngine;

public static class RandomHelper {

  public static bool NextBool() {
    return Random.value > 0.5f;
  }

  public static float NextGaussian() {
    return Mathf.Sqrt(-2.0f * Mathf.Log(Random.value)) *
           Mathf.Sin(2.0f * Mathf.PI * Random.value);
  }

  public static float NextGaussian(float stdev, float mean) {
    return NextGaussian() * stdev + mean;
  }

  // min: -15, max: 0
  // min: 0, max: 15
  public static float NextGaussianRange(float min, float max) {
    var stdev = (max - min) / 3f; // +-3 sigmas
    var mean = (Mathf.Abs(max) - Mathf.Abs(min)) / 2f;

    // Rejection sampling
    while (true) {
      var r = NextGaussian(stdev, mean);
      if (r >= min && r <= max) return r;
    }
  }

  public static float NextCauchy(float m, float gamma) {
    var a = NextGaussian();
    var b = NextGaussian();
    return m + gamma * 0.01f * (a / b);
  }

  public static float PoissonInput(float rate /* Hz */, float v) {
    var dt = 1f / 1000f; // 1ms
    return UnityEngine.Random.value < rate * dt ? v : 0.0f;
  }
}
