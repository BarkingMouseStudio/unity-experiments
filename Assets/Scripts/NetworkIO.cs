using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

// Responsible for marshalling input/output data to/from the neural network.
public class NetworkIO {

  public static readonly Range[] angularRanges = Range.From(new double[]{
    -180.0, -90.0, -45.0, -15.0, -5.0, -1.0, 0.0, 1.0, 5.0, 15.0, 45.0, 90.0, 180.0,
  });

  public static readonly Range[] linearRanges = Range.From(new double[]{
    -6.0, -3.0, -1.0, 0.0, 1.0, 3.0, 6.0,
  });

  public static readonly double[] speeds = new double[]{
    -200.0, -100.0, -10.0, -1.0, -0.1, 0.1, 1.0, 10.0, 100.0, 200.0
  };

  public static readonly Range[] inputRanges;

  public static readonly ulong[] inNeuronIds;
  public static readonly ulong[] outNeuronIds;

  public static readonly int inNeuronCount;
  public static readonly int outNeuronCount;

  public static readonly ulong MAX_DELAY = 20;

  static NetworkIO() {
    inNeuronCount = (NetworkIO.angularRanges.Length * 1) +
      (NetworkIO.linearRanges.Length * 1);
    outNeuronCount = NetworkIO.speeds.Length;

    // Set up input neuron ids by order
    inNeuronIds = Enumerable.Range(0, inNeuronCount)
      .Select(i => (ulong)i)
      .ToArray();

    // Set up output neuron ids by order _after_ input neuron ids
    outNeuronIds = Enumerable.Range(0, outNeuronCount)
      .Select(i => (ulong)(inNeuronCount + i))
      .ToArray();

    List<Range> inputRanges = new List<Range>(
      angularRanges.Length * 1 + linearRanges.Length * 1
    );
    inputRanges.AddRange(angularRanges); // theta lower
    // inputRanges.AddRange(angularRanges); // theta dot lower
    inputRanges.AddRange(linearRanges); // x
    // inputRanges.AddRange(linearRanges); // x dot
    NetworkIO.inputRanges = inputRanges.ToArray();

    Assert.AreEqual(inputRanges.Count, inNeuronCount);
  }

  readonly Neural.Network network;
  readonly int neuronCount;

  public NetworkIO(Neural.Network network) {
    this.network = network;
    this.neuronCount = (int)network.NeuronCount;
  }

  public float Send(float thetaLower, float x) {
    var aR = angularRanges.Length;
    // var aR2 = aR * 2;
    var lR = linearRanges.Length;
    // var lR2 = lR * 2;

    // Project world data
    var worldData = new float[aR + lR];
    for (int i = 0; i < worldData.Length; i++) {
      if (i < aR) {
        worldData[i] = thetaLower;
      // } else if (i >= aR && i < aR2) {
      //   worldData[i] = thetaDotLower;
      } else if (i >= aR && i < aR + lR) {
        worldData[i] = x;
      // } else if (i >= aR2 + lR && i < aR2 + lR2) {
      //   worldData[i] = xDot;
      }
    }

    // Filter world data by ranges
    double[] input = new double[neuronCount];
    Range range;
    float data;
    for (int i = 0; i < inputRanges.Length; i++) {
      range = inputRanges[i];
      data = worldData[i];
      if (range.Contains(data)) {
        input[inNeuronIds[i]] = 40.0 * range.Normalize(data);
      }
    }

    // Receive output
    // TODO: Compute real time scale
    var ticks = (ulong)(Time.fixedDeltaTime * 1000.0f);
    var output = new double[neuronCount];
    network.Tick(ticks, input, ref output);

    // Read out neuron V for speed
    float speed = 0.0f;
    for (int i = 0; i < speeds.Length; i++) {
      speed += (float)((output[outNeuronIds[i]] / 30.0) * speeds[i]);
    }
    return speed;
  }
}
