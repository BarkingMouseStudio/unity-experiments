using System.Collections;
using System.Collections.Generic;

public class NetworkInputPort {

  float[] input;
  IDictionary<int, IReceptiveField> rfs;

  public NetworkInputPort(float[] input, IDictionary<int, IReceptiveField> rfs) {
    this.input = input;
    this.rfs = rfs;
  }

  public void Set(float v) {
    foreach (var rf in rfs) {
      input[rf.Key] = rf.Value.Normalize(v) * 30.0f; // Will often be normalized to 0
    }
  }
}
