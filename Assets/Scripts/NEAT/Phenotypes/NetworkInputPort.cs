public class NetworkInputPort {

  Slice<double> input;
  IReceptiveField[] rfs;

  public NetworkInputPort(Slice<double> input, IReceptiveField[] rfs) {
    this.input = input;
    this.rfs = rfs;
  }

  public NetworkInputPort(Slicer<double> slicer, IReceptiveField[] rfs) {
    this.input = slicer.NextSlice(rfs.Length);
    this.rfs = rfs;
  }

  public void Set(double v) {
    for (int i = 0; i < input.Count; i++) {
      input[i] = rfs[i].Normalize(v) * 30.0; // Will often be normalized to 0
    }
  }
}
