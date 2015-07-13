using System.Linq;

public static class ArrayHelper {

  public static string Stringify<T>(T[] arr) {
    return string.Join(",", arr.Select(v => v.ToString()).ToArray());
  }
}
