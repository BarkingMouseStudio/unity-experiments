using System;
using System.Diagnostics;

public static class AssertHelper {

  public class AssertionException : Exception {

    public AssertionException(string message = "Condition failed.")
      : base(message) {}
  }

  [Conditional("DEBUG")]
  public static void Assert(bool condition) {
    if (!condition) {
      throw new AssertionException();
    }
  }

  [Conditional("DEBUG")]
  public static void Assert(bool condition, string message) {
    if (!condition) {
      throw new AssertionException(message);
    }
  }
}
