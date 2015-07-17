using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class IEnumerableExtensions {

  public static T[][] Batch<T>(this IEnumerable<T> source, int batchSize) {
    return source.Select((item, i) => {
      return new {
        Batch = Mathf.FloorToInt(i / batchSize),
        Item = item,
      };
    }).GroupBy(obj => {
      return obj.Batch;
    }).Select(grp => {
      return grp.Select(g => g.Item).ToArray();
    }).ToArray();
  }

  public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> func) {
    using (var a = first.GetEnumerator()) {
      using (var b = second.GetEnumerator()) {
        while (a.MoveNext() && b.MoveNext()) {
          yield return func(a.Current, b.Current);
        }
      }
    }
  }

  public static string Stringify<T>(this IEnumerable<T> source) {
    return string.Join(",", source.Select(v => v.ToString()).ToArray());
  }

  public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source, int N) {
    return source.Take(Math.Max(0, source.Count() - N));
  }

  public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int N) {
    return source.Skip(Math.Max(0, source.Count() - N));
  }
}
