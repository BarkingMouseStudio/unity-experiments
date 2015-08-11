using System.Collections;
using System.Collections.Generic;

public class EnumerableHelper {

  public static IEnumerable<double> Range(double start, double end, double stepby) {
    for (double val = start; val < end; val += stepby) {
      yield return val;
    }
  }

  public static IEnumerable<double> Range(double start, double end) {
    for (double val = start; val < end; val++) {
      yield return val;
    }
  }
}
