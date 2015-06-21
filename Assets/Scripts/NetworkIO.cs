// Responsible for marshalling input/output data to/from the neural network.
public class NetworkIO {

  static readonly Range[] angularRanges = Range.From(new double[]{
    -180.0, -90.0, -45.0, -15.0, -5.0, -1.0, 0.0, 1.0, 5.0, 15.0, 45.0, 90.0, 180.0,
  });

  static readonly Range[] linearRanges = Range.From(new double[]{
    -6.0, -3.0, -1.0, 0.0, 1.0, 3.0, 6.0,
  });

  static readonly double[] speeds = new double[]{
    -50.0, -5.0, -0.5, -0.05, 0.05, 0.5, 5.0, 50.0,
  };

  static readonly Range[] inputRanges = Range.From(new double[]{
    -180.0, -90.0, -45.0, -15.0, -5.0, -1.0, 0.0, 1.0, 5.0, 15.0, 45.0, 90.0, 180.0,
  }, new double[]{
    -180.0, -90.0, -45.0, -15.0, -5.0, -1.0, 0.0, 1.0, 5.0, 15.0, 45.0, 90.0, 180.0,
  }, new double[]{
    -6.0, -3.0, -1.0, 0.0, 1.0, 3.0, 6.0,
  }, new double[]{
    -6.0, -3.0, -1.0, 0.0, 1.0, 3.0, 6.0,
  });

  Neural.Network network;

  double[] data;
  double[] input;
  double[] output;

  ulong[] inIds;
  ulong[] outIds;

  int neuronCount;

  public NetworkIO(Neural.Network network) {
    this.network = network;

    // TODO: Clean this up:
    // Set up input neuron ids by order
    inIds = new ulong[data.Length];
    for (var i = 0; i < inIds.Length; i++) {
      inIds[i] = (ulong)i;
    }
    AssertHelper.Assert(inIds.Length == 36, "Unexpected input neuron id count");

    // Set up output neuron ids by order _after_ input neuron ids
    outIds = new ulong[speeds.Length];
    for (var i = 0; i < outIds.Length; i++) {
      outIds[i] = (ulong)(inIds.Length + i);
    }
    AssertHelper.Assert(outIds.Length == 8, "Unexpected output neuron id count");
    AssertHelper.Assert(outIds[0] == 36, "Unexpected first output neuron id");

    // Set up receiving data
    data = new double[angularRanges.Length * 2 + linearRanges.Length * 2];

    neuronCount = network.NeuronCount;
  }

  public void Send(float thetaLower, float thetaDotLower, float x, float xDot) {
    // TODO: Clean this up:
    // Lower world data into array
    int i = 0;
    int len;
    for (len = angularRanges.Length; i < len; i++) {
      data[i] = thetaLower;
    }
    for (len += angularRanges.Length; i < len; i++) {
      data[i] = thetaDotLower;
    }
    for (len += linearRanges.Length; i < len; i++) {
      data[i] = x;
    }
    for (len += linearRanges.Length; i < len; i++) {
      data[i] = xDot;
    }

    // Populate input array with data and ranges
    input = new double[neuronCount];

    Range range;
    for (i = 0; i < inIds.Length; i++) {
      range = inputRanges[i];
      if (range.Contains(data[i])) {
        input[inIds[i]] = 40.0f * range.Scale(data[i]);
      }
    }

    // Receive output
    var ticks = (ulong)(Time.fixedDeltaTime * 1000.0f);
    output = new double[neuronCount];
    network.Tick(ticks, input, ref output);

    // Read out neuron V for speed
    float speed = 0.0f;
    for (i = 0; i < outIds.Length; i++) {
      speed += (float)((output[outIds[i]] / 30.0) * speeds[i]);
    }
    return speed;
  }
}
