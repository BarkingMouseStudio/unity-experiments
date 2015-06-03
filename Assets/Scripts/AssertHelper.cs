using System;
using System.Diagnostics;

public static class AssertHelper {

  [Conditional("DEBUG")]
  public static void Assert(bool condition, string message) {
    if (!condition) {
      throw new Exception(message);
    }
  }
}
