using System;
using System.Collections;
using System.Collections.Generic;

public static class IEnumerableExtensions {
  public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> func) {
    using (var a = first.GetEnumerator()) {
      using (var b = second.GetEnumerator()) {
        while (a.MoveNext() && b.MoveNext()) {
          yield return func(a.Current, b.Current);
        }
      }
    }
  }
}
