using System.Collections;
using System.Collections.Generic;

public class NetworkSumOutputPort {

  double[] output;
  IDictionary<int, IReceptiveField> rfs;

  public NetworkSumOutputPort(double[] output, IDictionary<int, IReceptiveField> rfs) {
    this.output = output;
    this.rfs = rfs;
  }

  public double Get() {
    // Read out neuron V for sum
    double sum = 0.0f;
    foreach (var rf in rfs) {
      sum += rf.Value.Normalize(output[rf.Key] / 30.0);
    }
    return sum;
  }
}
