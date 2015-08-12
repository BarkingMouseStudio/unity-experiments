using System.Collections;
using System.Collections.Generic;

public class NetworkInputPort {

  double[] input;
  IDictionary<int, IReceptiveField> rfs;

  public NetworkInputPort(double[] input, IDictionary<int, IReceptiveField> rfs) {
    this.input = input;
    this.rfs = rfs;
  }

  public void Set(double v) {
    foreach (var rf in rfs) {
      input[rf.Key] = rf.Value.Normalize(v) * 30.0; // Will often be normalized to 0
    }
  }
}
