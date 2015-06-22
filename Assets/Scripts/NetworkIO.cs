using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Responsible for marshalling input/output data to/from the neural network.
public class NetworkIO {

  public static readonly Range[] angularRanges = Range.From(new double[]{
    -180.0, -90.0, -45.0, -15.0, -5.0, -1.0, 0.0, 1.0, 5.0, 15.0, 45.0, 90.0, 180.0,
  });

  public static readonly Range[] linearRanges = Range.From(new double[]{
    -6.0, -3.0, -1.0, 0.0, 1.0, 3.0, 6.0,
  });

  public static readonly double[] speeds = new double[]{
    -50.0, -5.0, -0.5, -0.05, 0.05, 0.5, 5.0, 50.0,
  };

  public static readonly Range[] inputRanges;

  static NetworkIO() {
    List<Range> inputRanges = new List<Range>(
      angularRanges.Length * 2 + linearRanges.Length * 2
    );
    inputRanges.AddRange(angularRanges); // theta lower
    inputRanges.AddRange(angularRanges); // theta dot lower
    inputRanges.AddRange(linearRanges); // x
    inputRanges.AddRange(linearRanges); // x dot
    NetworkIO.inputRanges = inputRanges.ToArray();
  }

  Neural.Network network;

  public NetworkIO(Neural.Network network) {
    this.network = network;
  }

  public float Send(float thetaLower, float thetaDotLower, float x, float xDot) {
    // Project world data
    var worldData = new List<float>(
      angularRanges.Length * 2 + linearRanges.Length * 2
    );
    worldData.AddRange(Enumerable.Repeat(thetaLower, angularRanges.Length));
    worldData.AddRange(Enumerable.Repeat(thetaDotLower, angularRanges.Length));
    worldData.AddRange(Enumerable.Repeat(x, linearRanges.Length));
    worldData.AddRange(Enumerable.Repeat(xDot, linearRanges.Length));

    // Apply to input ranges
    var input = worldData.Zip(inputRanges, (data, range) => {
      if (range.Contains(data)) {
        return 40.0f * range.Normalize(data);
      } else {
        return 0.0f;
      }
    }).ToArray();

    // Receive output
    var ticks = (ulong)(Time.fixedDeltaTime * 1000.0f);
    var output = new double[CommonGenotype.neuronCount];
    network.Tick(ticks, input, ref output);

    // Read out neuron V for speed
    float speed = 0.0f;
    int i = 0;
    foreach (var id in CommonGenotype.outNeuronIds) {
      speed += (float)((output[id] / 30.0) * speeds[i]);
      i++;
    }
    return speed;
  }
}
