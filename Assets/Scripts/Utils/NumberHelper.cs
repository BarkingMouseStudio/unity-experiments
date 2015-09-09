using UnityEngine;

public static class NumberHelper {

	public static float Bin(float v, float binSize) {
		return Mathf.Floor(v / binSize);
	}

  public static float Normalize(float x, float min, float max) {
    return (x - min) / (max - min);
  }

  public static float Scale(float x, float min, float max) {
    return (x * (max - min)) + min;
  }

  public static bool Between(float x, float min, float max) {
    return x > min && x < max;
  }
}
