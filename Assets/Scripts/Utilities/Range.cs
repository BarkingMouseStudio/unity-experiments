using System.Collections;
using System.Collections.Generic;
using System.Linq;

public struct Range {
  double start;
  double end;

  public Range(double start, double end) {
    this.start = start;
    this.end = end;
  }

  public bool Contains(double val) {
    return val >= start && val < end;
  }

  public double Normalize(double val) {
    return NumberHelper.Normalize(val, start, end);
  }

  public static Range Of(double start, double end) {
    return new Range(start, end);
  }

  public static Range[] From(double[] intervals) {
    var ranges = new List<Range>(intervals.Length - 1);
    for (var i = 0; i < intervals.Length - 1; i++) {
      ranges.Add(Range.Of(intervals[i], intervals[i + 1]));
    }
    return ranges.ToArray();
  }

  public static Range[] From(params double[][] intervals) {
    var ranges = new List<Range>();
    foreach (var interval in intervals) {
      ranges.AddRange(Range.From(interval));
    }
    return ranges.ToArray();
  }
}
