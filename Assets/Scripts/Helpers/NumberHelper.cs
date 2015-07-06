public static class NumberHelper {

  public static float Normalize(float x, float min, float max) {
    return (x - min) / (max - min);
  }

  public static double Normalize(double x, double min, double max) {
    return (x - min) / (max - min);
  }

  public static float Scale(float x, float min, float max) {
    return (x * (max - min)) + min;
  }
}
