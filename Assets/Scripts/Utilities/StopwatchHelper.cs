using System;
using System.Diagnostics;

public class StopwatchHelper : IDisposable {

  string name;
  Stopwatch stopwatch;

  public StopwatchHelper(string name) {
    this.name = name;
    this.stopwatch = Stopwatch.StartNew();
  }

  public void Dispose() {
    stopwatch.Stop();
    UnityEngine.Debug.LogFormat("{0} ({1}ms)", name, stopwatch.Elapsed.TotalMilliseconds);
  }
}
