public class NetworkExclusiveOutputPort {

  Slice<double> output;
  double[] values;

  public NetworkExclusiveOutputPort(Slice<double> output, double[] values) {
    this.output = output;
    this.values = values;
  }

  public NetworkExclusiveOutputPort(Slicer<double> slicer, double[] values) {
    this.output = slicer.NextSlice(values.Length);
    this.values = values;
  }

  public double Get() {
    return values[output.FindMaxIndex()];
  }
}
