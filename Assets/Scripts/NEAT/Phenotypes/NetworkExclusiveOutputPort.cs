public class NetworkExclusiveOutputPort {

  double[] output;
  double[] values;

  public NetworkExclusiveOutputPort(double[] output, double[] values) {
    this.output = output;
    this.values = values;
  }

  public double Get() {
    return values[output.FindMaxIndex()];
  }
}
