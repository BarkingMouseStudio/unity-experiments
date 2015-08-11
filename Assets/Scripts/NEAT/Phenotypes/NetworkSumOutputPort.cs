public class NetworkSumOutputPort {

  Slice<double> output;
  double[] multipliers;

  public NetworkSumOutputPort(Slice<double> output, double[] multipliers) {
    this.output = output;
    this.multipliers = multipliers;
  }

  public NetworkSumOutputPort(Slicer<double> slicer, double[] multipliers) {
    this.output = slicer.NextSlice(multipliers.Length);
    this.multipliers = multipliers;
  }

  public double Get() {
    // Read out neuron V for sum
    double sum = 0.0f;
    for (int i = 0; i < output.Count; i++) {
      sum += (output[i] / 30.0) * multipliers[i];
    }
    return sum;
  }
}
