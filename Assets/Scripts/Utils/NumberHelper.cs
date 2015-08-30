using UnityEngine;

public static class NumberHelper {

	public static float Bin(float v, float binSize) {
		return Mathf.Floor(v / binSize);
	}

  public static float Normalize(float x, float min, float max) {
    return (x - min) / (max - min);
  }

  public static double Normalize(double x, double min, double max) {
    return (x - min) / (max - min);
  }

  public static float Scale(float x, float min, float max) {
    return (x * (max - min)) + min;
  }

  public static bool Between(float x, float min, float max) {
    return x > min && x < max;
  }

  public static bool Between(double x, double min, double max) {
    return x > min && x < max;
  }

  public static double Clamp(double x, double min, double max) {
    if (x > max) {
      return max;
    } else if (x < min) {
      return min;
    } else {
      return x;
    }
  }

  public static double Clamp01(double x) {
    return Clamp(x, 0.0, 1.0);
  }
}
