using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

  public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source, int N) {
    return source.Take(Math.Max(0, source.Count() - N));
  }

  public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int N) {
    return source.Skip(Math.Max(0, source.Count() - N));
  }
}