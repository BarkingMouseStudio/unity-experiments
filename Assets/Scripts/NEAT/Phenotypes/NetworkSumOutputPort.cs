using System.Collections;
using System.Collections.Generic;

public class NetworkSumOutputPort {

  float[] output;
  IDictionary<int, IReceptiveField> rfs;

  public NetworkSumOutputPort(float[] output, IDictionary<int, IReceptiveField> rfs) {
    this.output = output;
    this.rfs = rfs;
  }

  public float Get() {
    // Read out neuron V for sum
    float sum = 0.0f;
    foreach (var rf in rfs) {
      sum += rf.Value.Normalize(output[rf.Key] / 30.0f);
    }
    return sum;
  }
}
