public static class AngleHelper {

  public static float GetAngle(float angle) {
  	while (angle > 180.0f) {
      angle -= 360.0f;
    }
  	while (angle < -180.0f) {
      angle += 360.0f;
    }
  	return angle;
  }
}
